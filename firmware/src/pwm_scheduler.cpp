#include <pwm_scheduler.h>
// Declare friend function prototype.
void set_new_ttl_pin_state(void);
void handle_missed_deadline();

// Define static variable. These should not be in flash such that they
// can be accessed by the ISR quickly.
volatile int32_t __not_in_flash("alarm_num") PWMScheduler::alarm_num_ = -1;
volatile bool __not_in_flash("alarm_queued") PWMScheduler::alarm_queued_ = false;
volatile uint32_t __not_in_flash("next_gpio_port_mask") PWMScheduler::next_gpio_port_mask_ = 0;
volatile uint32_t __not_in_flash("next_gpio_port_state") PWMScheduler::next_gpio_port_state_ = 0;

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
{
    reset();
}

void PWMScheduler::reset()
{
    cancel_alarm(); // Cancel any upcoming alarms.
    pq_.clear(); // Remove all tasks in the priority queue.
}

void PWMScheduler::start()
{
    // Note: sorting the schedule for the first time incurs a longer delay
    // than it otherwise would during normal updates.
    uint32_t start_time_us = timer_hw->timerawl + 1000; // Schedule 1[ms] into the future.
    //printf("starting schedule at: %lld.\r\n", now);
    // PWMTasks have a default start time of 0, so popping each task to update
    // it is an OK way to iterate through the priority queue at the start.
    for (uint8_t i = 0; i < pq_.size(); ++i)
    {
        PWMTask& task = pq_.top().get();
        pq_.pop();
        //printf("Task %d old start time: %lld || ", task.pin(), task.start_time_us());
        task.start_at_time(start_time_us, true); // skip initial output action.
        //printf("new start time: %lld\r\n", task.start_time_us());
        pq_.push(task); // reschedule at the new time.
    }
    // Schedule the initial pin state. (masks were already updated upon
    // pushing them into the pq_.)
    alarm_queued_ = true; // Do this first in case alarm fires immediately.
    timer_hw->inte |= (1u << alarm_num_); // enable alarm to trigger interrupt.
    timer_hw->alarm[alarm_num_] = start_time_us; // write time (also arms alarm).

/*
    uint32_t now = timer_hw->timerawl;
    printf("Starting schedule at : %lu, curr time: %lu\r\n", start_time_us, now);
*/
}

void PWMScheduler::clear()
{}

void PWMScheduler::update()
{
    // Prevent queuing another alarm until the port state change has occured.
    // Bail early if there are no tasks.
    if (alarm_queued_ || (pq_.size() == 0))
        return;
    next_gpio_port_mask_ = 0;
    next_gpio_port_state_ = 0;
    uint32_t next_pwm_task_update_time_us = pq_.top().get().next_update_time_us_;
    for (uint8_t i = 0; i < NUM_TTL_IOS; ++i)
    {
        // Pop the highest priority (must update soonest) PWM task.
        PWMTask& pwm = pq_.top().get();
        pq_.pop();
        // Update this PWM state and the next time that it needs to be called.
        // Skip gpio action since we will fire all pins of all PWMTasks at once.
        pwm.update(true, true); // force = true; skip_output_action = true.
        // Update the queued gpio port state.
        next_gpio_port_mask_ |= pwm.pin_mask_;
        if (pwm.state_ == PWMTask::update_state_t::HIGH)
            next_gpio_port_state_ |= pwm.pin_mask_;
        // Put this task back in the pq if it must be updated later.
        if (pwm.requires_future_update())
            pq_.push(pwm);
        // Continue scheduling all PWM tasks that will fire simultaneously.
        if (pq_.size() == 0)
            break;
        if (pq_.top().get().next_update_time_us_ != next_pwm_task_update_time_us)
            break;
    }
/*
    // Save next update time although we currently don't use it.
    if (pq_.size())
        next_update_time_us_ = pq_.top().get().next_update_time_us_;
*/
    // Schedule the GPIO port state change!
    // Schedule alarm with ptr to class instance as a parameter.
    // TODO: figure out how to disambiguate Harp/Pico time.
    // Edge case: detect if we have fallen behind.
    // Note: we can't really recover from being behind "once" because we will
    //  continue to fall behind.
    uint32_t timer_raw = timer_hw->timerawl;
    if (int32_t(timer_raw - next_pwm_task_update_time_us) > 0)
    {
#ifdef DEBUG
        printf("Deadline missed! Curr time: %lu | scheduled time: %lu\r\n",
               timer_raw, next_pwm_task_update_time_us);
#endif
        handle_missed_deadline();
    }
    // Normal case: arm the alarm and let the interrupt apply the state change.
    alarm_queued_ = true; // Do this first in case alarm fires immediately.
    timer_hw->inte |= (1u << alarm_num_); // enable alarm to trigger interrupt.
    timer_hw->alarm[alarm_num_] = next_pwm_task_update_time_us; // write time
                                                                // (also arms alarm)
}

// Put the ISR in RAM so as to avoid (slow) flash access.
void __not_in_flash_func(set_new_ttl_pin_state)(void)
{
    // Apply the next GPIO state.
    gpio_put_masked(PWMScheduler::next_gpio_port_mask_,
                    PWMScheduler::next_gpio_port_state_);
    PWMScheduler::alarm_queued_ = false;
    // Clear the latched hardware interrupt.
    timer_hw->intr |= (1u << PWMScheduler::alarm_num_);
}

void __attribute__((weak)) handle_missed_deadline()
{
    while(1); // block forever by default.
}
