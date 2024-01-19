#ifndef PULSE_SCHEDULER_H
#define PULSE_SCHEDULER_H
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/irq.h>
#include <pwm_task.h>
#include <etl/priority_queue.h>
#ifdef DEBUG
#include <cstdio> // for printf
#endif


#define NUM_ENTRIES (64)
#define NUM_TTL_IOS (8)


class PWMScheduler
{
public:
    PWMScheduler();
    ~PWMScheduler();

    bool schedule(PortEvent event);
    void start();
    void clear();

    friend int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

/**
 * \brief called periodically. Sets up next PortEvent to occur on a timer.
 */
    void update();

private:
    etl::vector<std::reference_wrapper<PortEvent>, 8> next_events_; /// container for next simultaneous events.
    etl::priority_queue<PWMTask,
                        NUM_ENTRIES,
                        etl::vector<PWMTask, NUM_ENTRIES>,
                        etl::greater<PWMTask>> pq_;

    volatile uint32_t next_gpio_port_state_;
    volatile uint32_t next_gpio_port_mask_;
};
#endif // PULSE_SCHEDULER_H
