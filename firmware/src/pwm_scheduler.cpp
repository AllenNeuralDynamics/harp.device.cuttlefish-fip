#include <pwm_scheduler.h>
// Declare friend function prototype.
void set_new_ttl_pin_state(void);
void handle_missed_deadline();

// Define static variables. These should not be in flash such that they
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
    // Attach interrupt to function and enable interrupt.
    irq_set_exclusive_handler(irq_num, set_new_ttl_pin_state);
    irq_set_enabled(irq_num, true);
    timer_hw->inte |= (1u << alarm_num_); // enable Alarm to trigger interrupt.
}

PWMScheduler::~PWMScheduler()
{
    reset();
}

void PWMScheduler::reset()
{
#if defined(DEBUG)
        printf("Resetting PWMScheduler... (pq_ size: %d | pwm_tasks_ size: %d)\r\n",
                pq_.size(), pwm_tasks_.size());
#endif
    cancel_alarm(); // Cancel any upcoming alarms.
    pq_.clear(); // Remove all tasks in the priority queue.
    pwm_tasks_.clear();
#if defined(DEBUG)
        printf("Done resetting PWMScheduler.\r\n");
#endif
}

void PWMScheduler::schedule_pwm_task(PWMTask task)
{
    schedule_pwm_task(task.delay_us_, task.on_time_us_, task.period_us_,
                      task.pin_mask_, task.count_, task.invert_);
}

void PWMScheduler::schedule_pwm_task(uint32_t delay_us, uint32_t t_on_us,
                                     uint32_t t_period_us, uint32_t pin_mask,
                                     uint32_t count, bool invert)
{
    // Create PWMTask and push into the vector.
    pwm_tasks_.emplace_back(delay_us, t_on_us, t_period_us, pin_mask, count,
                            invert);
    PWMTask& task = pwm_tasks_.back();
    // Aggreggate initial pin state vector.
    // TODO: check this (inversion case, delayed start case).
    next_gpio_port_mask_ |= task.pin_mask_;
    if (task.starting_state() == PWMTask::update_state_t::HIGH)
        next_gpio_port_state_ |= task.pin_mask_;
    pq_.push(pwm_tasks_.back()); // PWMTasks are *sorted* since comparison is
                                 // based on an unspecified (and therefore
                                 // relative) t=0 start time.
#ifdef DEBUG
    printf("Pushed PWMTask: (%d, %d, %d, 0x%08x)\r\n", task.delay_us_,
           task.on_time_us_, task.period_us_, task.pin_mask_);
#endif
}

void PWMScheduler::start()
{
    // Note: schedule is pre-sorted and first GPIO state is pre-set.
    // Save schedule start time.
    uint32_t start_time_us = timer_hw->timerawl;
    // Apply initial pending GPIO change immediately so the schedule starts now.
    // Note: starting GPIO state was aggregated when we add each PWMTask.
    gpio_put_masked(next_gpio_port_mask_, next_gpio_port_state_);
    printf("GPIO Put: 0x%08x (mask), 0x%08x (val)\r\n",
           next_gpio_port_mask_, next_gpio_port_state_);
    // Set starting time of all PWMTasks.
    // Note that tasks are pre-sorted at this point bc they are sorted upon
    //  being stored.
    for (auto& task: pwm_tasks_)
    {
        task.reset(true); // Reset to starting state but do not drive pin output.
        task.set_time_started(start_time_us);
    }
#if defined(DEBUG)
    printf("Recording schedule start at : %lu\r\n", start_time_us);
#endif
}

void PWMScheduler::update(bool first_update)
{
    // Prevent queuing another alarm until the port state change has occured.
    // Bail early if there are no tasks.
    if (alarm_queued_ || (pq_.size() == 0))
        return;
#if defined(DEBUG)
    uint32_t start_time_us = timer_hw->timerawl;
    printf("Updating schedule at : %lu\r\n", start_time_us);
#endif
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
        // Update the queued gpio port state;
        next_gpio_port_mask_ |= pwm.pin_mask_;
        if (pwm.state_ == PWMTask::update_state_t::HIGH)
            next_gpio_port_state_ |= pwm.pin_mask_;
        // Put this task back in the pq if it must be updated later.
        if (pwm.requires_future_update())
            pq_.push(pwm);
        if (pq_.size() == 0)
            break;
        // Continue scheduling all PWM tasks that will fire simultaneously.
        if (pq_.top().get().next_update_time_us_ != next_pwm_task_update_time_us)
            break;
    }
    uint32_t alarm_time_us = next_pwm_task_update_time_us;
#if defined(DEBUG)
    printf("Updating done. ISR set for %lu | Next update at : %lu\r\n",
            alarm_time_us, alarm_time_us);
#endif
    // TODO: figure out how to disambiguate Harp/Pico time.
    // Schedule the GPIO port state change.
    // Edge case: detect if we have fallen behind.
    // Note: we can't really recover from being behind "once" because we will
    //  continue to fall behind.
    uint32_t timer_raw = timer_hw->timerawl;
    if (int32_t(timer_raw - alarm_time_us) > 0)
    {
#if defined(DEBUG)
        printf("Deadline missed! Curr time: %lu | scheduled time: %lu | delta: %lu\r\n",
               timer_raw, alarm_time_us, int32_t(timer_raw - alarm_time_us));
#endif
        handle_missed_deadline();
    }
    // Normal case: arm the alarm and let the interrupt apply the state change.
    alarm_queued_ = true; // Do this first in case alarm fires immediately.
    timer_hw->alarm[alarm_num_] = alarm_time_us; // write time (also arms alarm)
}

// FIXME: implement stop so that we can restart without reloading a schedule.
/*
void PWMScheduler::stop()
{
    cancel_alarm();
    pq_.clear(); // Remove all tasks in the priority queue.
    // Set the starting state of all GPIO pins if we restart.
    next_gpio_port_mask_ = 0;
    for (auto& task: pwm_tasks_)
    {
        pwm_task.stop(); // Kill GPIO output. // Clear task start time.
        task.reset(); // Clear internal counters. Disable GPIO
        next_gpio_port_mask_ |= task.pin_mask_;
        if (task.starting_state() == PWMTask::update_state_t::HIGH)
            next_gpio_port_state_ |= task.pin_mask_;
        pq_.push(pwm_tasks_.back()); // pushes task with unset "t=0" time.
    }
}
*/

void PWMScheduler::cancel_alarm()
{
    timer_hw->armed |= (1u << alarm_num_);
    alarm_queued_ = false;
}

// Put the ISR in RAM so as to avoid (slow) flash access.
void __not_in_flash_func(set_new_ttl_pin_state)(void)
{
    // Apply the next GPIO state.
    gpio_put_masked(PWMScheduler::next_gpio_port_mask_,
                    PWMScheduler::next_gpio_port_state_);
/*
    printf("GPIO Put: 0x%08x (mask), 0x%08x (val)\r\n",
           PWMScheduler::next_gpio_port_mask_, PWMScheduler::next_gpio_port_state_);
*/
    PWMScheduler::alarm_queued_ = false;
    // Clear the latched hardware interrupt.
    timer_hw->intr |= (1u << PWMScheduler::alarm_num_);
}

void __attribute__((weak)) handle_missed_deadline()
{
    while(1); // block forever by default.
}
