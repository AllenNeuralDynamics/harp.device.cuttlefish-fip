#include <pwm_scheduler.h>
// Declare friend function prototype.
int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

PWMScheduler::PWMScheduler()
: alarm_queued_{false}
{}

PWMScheduler::~PWMScheduler()
{}

void PWMScheduler::start()
{
    uint64_t now = time_us_64(); // TODO: should be Harp time?
    //printf("starting schedule at: %lld.\r\n", now);
    // PWMTasks have a default start time of 0, so popping each task to start
    // it is an OK way to iterate through the priority queue at the start.
    for (uint8_t i = 0; i < pq_.size(); ++i)
    {
        PWMTask& task = pq_.top().get();
        pq_.pop();
        //printf("Task %d old start time: %lld || ", task.pin(), task.start_time_us());
        task.start_at_time(now);
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
    uint64_t next_pwm_task_update_time_us = pq_.top().get().next_update_time_us();
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
        next_gpio_port_state_ &= ~pwm.pin_mask();
        next_gpio_port_state_ |= (pwm.state() == PWMTask::update_state_t::HIGH)?
                                    pwm.pin_mask(): 0;
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
    add_alarm_at(next_pwm_task_update_time_us, set_new_ttl_pin_state, this, true);
}

// Put the ISR in RAM so as to avoid flash access.
int64_t __not_in_flash_func(set_new_ttl_pin_state)(alarm_id_t id, void* user_data)
{
    PWMScheduler& self = *((PWMScheduler*)user_data);
    gpio_put_masked(self.next_gpio_port_mask_, self.next_gpio_port_state_);
    self.alarm_queued_ = false;
    return 0; // Do not reschedule.
}
