#include <pulse_scheduler.h>


PulseScheduler::PulseScheduler()
{}


PulseScheduler::~PulseScheduler()
{}

bool PulseScheduler::schedule(PortEvent event)
{
    return true;
}


void PulseScheduler::start()
{}

void PulseScheduler::clear()
{}


void PulseScheduler::update()
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
        // peek.
        // break if needed.
    }
        for (auto& event: next_events_)
            pq_.push(event);
    // Schedule!
}
