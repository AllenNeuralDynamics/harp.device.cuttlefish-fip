#include <pulse_scheduler.h>
// Declare friend function prototype.
int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

PWMScheduler::PWMScheduler(uint8_t ttl_base_pin, uint8_t dir_base_pin)
:ttl_base_pin_{ttl_base_pin}, dir_base_pin_{dir_base_pin}
{
    // TODO: gpio initialization here.
}

PWMScheduler::~PWMScheduler()
{}

bool PWMScheduler::schedule(PortEvent event)
{
    return true;
}

void PWMScheduler::start()
{}

void PWMScheduler::clear()
{}


void PWMScheduler::update()
{
    // Pop the scheduled event.
    // update the queued gpio port state.
    // for pin in active_ pins
    //     peek into the future.
    //     if next scheduled event is not at the same scheduled time,
    //         break;
    //     pop the scheduled element.
    //     update the queued gpio port state
    // reschedule all popped events.
    // set alarm for updating port state.
    // peek into next event and sleep until that time.
    for (uint8_t i = 0; i < NUM_TTL_IOS; ++i)
    {
        PortEvent& next_event = pq_.top();
        pq_.pop();
        next_events_.push_back(next_event);
        next_gpio_port_state_ &= ~next_event.port_mask;
        next_gpio_port_state_ |= next_event.port_state;
        next_gpio_port_mask_ |= next_event.port_mask;
        next_event.update(); // update the time that this event needs to be called next.
        // peek.
        // break if needed.
    }
        for (auto& event: next_events_)
            pq_.push(event);
    // Schedule! Note. times are relative and probably in harp time??
    // Schedule alarm with ptr to class instance as a parameter.
    uint64_t alarm_time_us = time_us_64() + 10;//HarpCore::time_us_64();
    add_alarm_at(alarm_time_us, set_new_ttl_pin_state, this, true);
}

int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data)
{
    PWMScheduler& self = *((PWMScheduler*)user_data);
    gpio_put_masked((self.next_gpio_port_mask_ << self.ttl_base_pin_),
                    (self.next_gpio_port_state_ << self.ttl_base_pin_));
    return 0; // Do not reschedule.
}
