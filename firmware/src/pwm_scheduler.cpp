#include <pwm_scheduler.h>
// Declare friend function prototype.
int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

PWMScheduler::PWMScheduler()
:alarm_queued_{false}
{}

PWMScheduler::~PWMScheduler()
{}

void PWMScheduler::start()
{
    uint64_t now = time_us_64(); // TODO: should be Harp time?
    printf("starting schedule at: %lld.\r\n", now);
    // Iterating through priority queue elements is clunky.
    PWMTask* base = &pq_.top();
    for (uint8_t i = 0; i < pq_.size(); ++i)
        (*(base + i)).start_at_time(now);
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
    // Pop the highest priority PWM task.
    PWMTask* pwm = &pq_.top();
    PWMTask* next_pwm;
    uint64_t next_pwm_task_update_time_us = pwm->next_update_time_us();
    //printf("next update time for pin %d is %lld.\r\n", pwm->pin_, next_pwm_task_update_time_us);
    for (uint8_t i = 0; i < NUM_TTL_IOS; ++i)
    {
        pq_.pop();
        // Save to a vector of all PWM tasks that need to execute concurrently.
        next_tasks_.push_back(*pwm);
        // Update this PWM state and the next time that it needs to be called.
        // Skip gpio action since we will fire all pins of all PWMTasks at once.
        pwm->update(true, true); // force = true; skip_output_action = true.
        // Update the queued gpio port state.
        next_gpio_port_mask_ |= (1u << pwm->pin_);
        next_gpio_port_state_ &= ~(1u << pwm->pin_);
        next_gpio_port_state_ |= (pwm->state_ << pwm->pin_);
        // Continue scheduling all PWM tasks that will fire simultaneously.
        // Bail if no more tasks exist or the next task fires after the set of
        // current tasks.
        if (pq_.size() == 0)
        {
            //printf("Broke at iteration %d\r\n", i);
            break;
        }
        next_pwm = &pq_.top();
        if (next_pwm->next_update_time_us() != next_pwm_task_update_time_us)
            break;
        pwm = next_pwm;
    }
    // Reschedule all updated SoftPWMs that will require a future update.
    for (auto& pwm_ref: next_tasks_)
    {
        if (pwm_ref.get().requires_future_update())
            pq_.push(pwm_ref.get());
    }
    next_tasks_.clear();
    //printf("queueing alarm at %lld  || ", next_pwm_task_update_time_us);
    //printf("mask: 0x%x, state: 0x%x\r\n", next_gpio_port_mask_, next_gpio_port_state_);
    // Track (1) that we're queueing an alarm and (2) the next time the
    // scheduler neeeds to be updated. Do this first in case the alarm fires
    // immediately.
    alarm_queued_ = true;
    next_update_time_us_ = pq_.top().next_update_time_us();
    // Schedule the GPIO port state change!
    // Schedule alarm with ptr to class instance as a parameter.
    // TODO: figure out how to disambiguate Harp/Pico time.
    add_alarm_at(next_pwm_task_update_time_us, set_new_ttl_pin_state, this, true);
}

int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data)
{
    PWMScheduler& self = *((PWMScheduler*)user_data);
    gpio_put_masked(self.next_gpio_port_mask_, self.next_gpio_port_state_);
    self.alarm_queued_ = false;
    //printf("fired.\r\n");
    return 0; // Do not reschedule.
}
