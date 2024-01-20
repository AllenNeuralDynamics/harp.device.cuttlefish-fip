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
    // Iterating through priority queue elements is clunky.
    PWMTask* base = &pq_.top();
    for (uint8_t i = 0; i < pq_.size(); ++i)
        (*(base + i)).start_at_time(now);
}

void PWMScheduler::clear()
{}


void PWMScheduler::update()
{
    if (alarm_queued_)
        return;
    // Pop the highest priority PWM task.
    PWMTask* pwm = &pq_.top();
    PWMTask* next_pwm;
    uint64_t next_update_time_us = pwm->next_update_time_us();
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
        next_gpio_port_state_ |= (1u << pwm->state());
        // Continue scheduling all PWM tasks that will fire simultaneously.
        next_pwm = &pq_.top();
        if (next_pwm->next_update_time_us() != next_update_time_us)
            break;
        pwm = next_pwm;
    }
    // Reschedule all updated SoftPWMs that will require a future update.
        for (auto& pwm_ref: next_tasks_)
        {
            if (pwm_ref.get().requires_future_update())
                pq_.push(pwm_ref);
        }
    // Schedule the GPIO port state change!
    // TODO: figure out how to disambiguate Harp/Pico time.
    // Schedule alarm with ptr to class instance as a parameter.
    add_alarm_at(next_update_time_us, set_new_ttl_pin_state, this, true);
    alarm_queued_ = true;
    next_update_time_us = next_pwm->next_update_time_us();
}

int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data)
{
    PWMScheduler& self = *((PWMScheduler*)user_data);
    gpio_put_masked(self.next_gpio_port_mask_, self.next_gpio_port_state_);
    self.alarm_queued_ = false;
    return 0; // Do not reschedule.
}
