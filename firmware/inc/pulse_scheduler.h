#ifndef PULSE_SCHEDULER_H
#define PULSE_SCHEDULER_H
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/irq.h>
#include <etl/priority_queue.h>
#ifdef DEBUG
#include <cstdio> // for printf
#endif


#define NUM_ENTRIES (64)
#define NUM_TTL_IOS (8)


class PulseScheduler
{
public:
    struct PortEvent
    {
        uint32_t port_mask; // which pins.
        uint32_t port_state; // what state the pins are in.
        uint32_t timestamp_us; // when the state takes place.
        /**
         * \brief "less-than" operator for implementing task sorting.
         * \note etl::compare only requres "less than" to be implemented.
         * \note Signed subtraction handles uint32_t rollover.
         */
        friend bool operator<(const PortEvent& lhs, const PortEvent& rhs)
        {return int32_t(rhs.timestamp_us - lhs.timestamp_us) > 0;}
    };

    PulseScheduler();
    ~PulseScheduler();

    bool schedule(PortEvent event);
    void start();
    void clear();

/**
 * \brief called periodically. Sets up next PortEvent to occur on a timer.
 */
    void update();

private:
    etl::vector<std::reference_wrapper<PortEvent>, 8> next_events_; /// container for next simultaneous events.
    etl::priority_queue<PortEvent,
                        NUM_ENTRIES,
                        etl::vector<PortEvent, NUM_ENTRIES>,
                        etl::greater<PortEvent>> pq_;

    volatile uint32_t next_gpio_port_state_;
};
#endif // PULSE_SCHEDULER_H
