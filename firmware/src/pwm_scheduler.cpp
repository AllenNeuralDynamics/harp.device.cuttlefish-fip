#include <pulse_scheduler.h>
// Declare friend function prototype.
int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

PWMScheduler::PWMScheduler()
{}

PWMScheduler::~PWMScheduler()
{}

bool PWMScheduler::schedule(PortEvent event)
{
    return true;
}

void PWMScheduler::start()
{
    uint64_t now = time_us_64(); // TODO: should be Harp time?
    for (auto& pwm_task: pq_)
        pwm_task.start_at_time(now);
}

void PWMScheduler::clear()
{}


void PWMScheduler::update()
{
    // for pin in active_ pins
    //         break;
    //     pop the scheduled element.
    //     update the queued gpio port state
    // reschedule all popped events.
    // set alarm for updating port state.
    // peek into next event and sleep until that time.

    // Pop the highest priority PWM task.
    PWMTask& pwm = pq_.top();
    uint64_t next_update_time_s = next_update_time_us;
    for (uint8_t i = 0; i < NUM_TTL_IOS; ++i)
    {
        pq_.pop();
        // Save to a vector of all PWM tasks that need to execute concurrently.
        next_events_.push_back(pwm);
        // Update this PWM state and the next time that it needs to be called.
        // Skip gpio action since we will fire all pins of all PWMTasks at once.
        pwm.update(true, true); // force = true; skip_output_action = true.
        // Update the queued gpio port state.
        next_gpio_port_mask_ |= (1u << pwm.pin);
        next_gpio_port_state_ &= ~(1u << pwm.pin);
        next_gpio_port_state_ |= (1u << pwm.state);
        // Continue scheduling all PWM tasks that will fire simultaneously.
        PWMTask& next_pwm = pt_.top();
        if (next_pwm.next_update_time != pwm.next_update_time)
            break;
        pwm = next_pwm;
    }
    // Reschedule all SoftPWMs that we just updated.
        for (auto& event: next_events_)
            pq_.push(event);
    // Schedule the GPIO port state change!
    // FIXME: figure out how to disambiguate Harp/Pico time.
    // Schedule alarm with ptr to class instance as a parameter.
    add_alarm_at(next_update_time_us, set_new_ttl_pin_state, this, true);
}

int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data)
{
    PWMScheduler& self = *((PWMScheduler*)user_data);
    gpio_put_masked(self.next_gpio_port_mask_, self.next_gpio_port_state_);
    return 0; // Do not reschedule.
}
