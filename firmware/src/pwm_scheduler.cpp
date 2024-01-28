#include <pwm_scheduler.h>
// Declare friend function prototype.
void set_new_ttl_pin_state(void);

// Define static variables.
volatile int32_t PWMScheduler::alarm_num_ = -1;
volatile bool PWMScheduler::alarm_queued_ = false;
volatile uint32_t PWMScheduler::next_gpio_port_mask_ = 0;
volatile uint32_t PWMScheduler::next_gpio_port_state_ = 0;

PWMScheduler::PWMScheduler()
{
    // Claim alarm via this function call so the pico sdk doesn't use it.
    // Don't claim if it has already been claimed.
    if (alarm_num_ < 0)
        alarm_num_ = hardware_alarm_claim_unused(true); // required = true;
    uint32_t irq_num = TIMER_IRQ_0 + alarm_num_; //hardware_alarm_irq_number(alarm_num_);
    // Attach interrupt to function and enable alarm to generate interrupt.
    irq_set_exclusive_handler(irq_num, set_new_ttl_pin_state);
    irq_set_enabled(irq_num, true);
}

PWMScheduler::~PWMScheduler()
{}

void PWMScheduler::start()
{
    // Fire the initial state.
    uint32_t now = timer_hw->timerawl + 1000000;
    set_new_ttl_pin_state();
    //printf("starting schedule at: %lld.\r\n", now);
    // PWMTasks have a default start time of 0, so popping each task to update
    // it is an OK way to iterate through the priority queue at the start.
    for (uint8_t i = 0; i < pq_.size(); ++i)
    {
        PWMTask& task = pq_.top().get();
        pq_.pop();
        //printf("Task %d old start time: %lld || ", task.pin(), task.start_time_us());
        task.start_at_time(now, true); // skip initial output action.
        //printf("new start time: %lld\r\n", task.start_time_us());
        pq_.push(task); // reschedule at the new time.
    }
}

void PWMScheduler::clear()
{}

void PWMScheduler::update()
{
    // Prevent queuing another alarm until the port state change has occured.
    if (alarm_queued_)
        return;
    next_gpio_port_mask_ = 0;
    next_gpio_port_state_ = 0;
    uint32_t next_pwm_task_update_time_us = pq_.top().get().next_update_time_us();
    for (uint8_t i = 0; i < NUM_TTL_IOS; ++i)
    {
        // Pop the highest priority (must update soonest) PWM task.
        PWMTask& pwm = pq_.top().get();
        pq_.pop();
        // Update this PWM state and the next time that it needs to be called.
        // Skip gpio action since we will fire all pins of all PWMTasks at once.
        pwm.update(true, true); // force = true; skip_output_action = true.
        // Update the queued gpio port state.
        next_gpio_port_mask_ |= pwm.pin_mask();
        if (pwm.state() == PWMTask::update_state_t::HIGH)
            next_gpio_port_state_ |= pwm.pin_mask();
        // Put this task back in the pq.
        pq_.push(pwm);
        // Continue scheduling all PWM tasks that will fire simultaneously.
        if (pq_.top().get().next_update_time_us() != next_pwm_task_update_time_us)
            break;
    }
    // Track (1) that we're queueing an alarm and (2) the next time the
    // scheduler neeeds to be updated. Do this first in case the alarm fires
    // immediately.
    alarm_queued_ = true;
    next_update_time_us_ = pq_.top().get().next_update_time_us();
    // Schedule the GPIO port state change!
    // Schedule alarm with ptr to class instance as a parameter.
    // TODO: figure out how to disambiguate Harp/Pico time.
    // Note: we can't really recover from being behind "once" because will will
    //  always continue to fall behind.
    if (int32_t(timer_hw->timerawl - next_pwm_task_update_time_us) > 0)
    {
        //set_new_ttl_pin_state();
#ifdef DEBUG
        printf("missed our deadline!\r\n");
#endif
        while(1);
        //return;
    }
    // Normal case: arm the alarm and let the interrupt apply the state change.
    timer_hw->inte |= (1u << alarm_num_); // enable alarm to trigger interrupt.
    timer_hw->alarm[alarm_num_] = next_pwm_task_update_time_us; // write time
                                                                         // (also arms alarm)
}

// Put the ISR in RAM so as to avoid flash access (slow).
void __not_in_flash_func(set_new_ttl_pin_state)(void)
{
    // Apply the next GPIO state.
    gpio_put_masked(PWMScheduler::next_gpio_port_mask_,
                    PWMScheduler::next_gpio_port_state_);
    PWMScheduler::alarm_queued_ = false;
    // Clear the latched hardware interrupt.
    timer_hw->intr |= (1u << PWMScheduler::alarm_num_);
}
