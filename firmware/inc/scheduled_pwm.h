#ifndef SCHEDULED_PWM_H
#define SCHEDULED_PWM_H
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/irq.h>
#include <etl/priority_queue.h>
#ifdef DEBUG
#include <cstdio> // for printf
#endif

class ScheduledPWM
{
public:
    ScheduledPWM(uint32_t t_delay_us, uint32_t t_rise_us,
                 uint32_t t_fall_us, uint32_t t_period_us,
                 uint32_t pin_mask, uint32_t count = 0,
                 bool invert = false);

    ~ScheduledPWM
    enum update_state_t: uint8_t
    {
        LOW = 0,
        HIGH = 1
    }
    uint32_t delay_us_;
    uint32_t on_time_us_; // effectively duty_cycle
    uint32_t period_us_;

    uint32_t pin_mask_; // active channels.

    // FIXME: we need an internal representation of what time this FSM started?
    //  We also need to be able to set it with "start" or "set_started_at_time(uint32_t time)"
    // FIXME: we need a comparison operator to sort which ScheduledPWM instance
    //  should update next..

    void update();

    inline uint32_t next_update_time(){return 0}; // FIXME: what is this?

    inline void start()
    { reset();}

/**
 * \brief start the PWM state machine but override its start time to
 *  schedule it in the future.
 */
    inline void start_at_time(uint32_t start_time_us)
    {
        start_time_us_ = start_time_us;
        start();
    }

private:
    uint32_t count_; // N==0: pulse forever. N>0: execute N times.
    uint32_t cycles_; // how many times we have pulsed.
    update_state_t state;
    uint32_t start_time_us_;
    uint32_t next_update_time_us_; // relative to the start time.

    inline void reset()
    {   state = LOW;
        next_update_time_us_ = start_time_us_ + delay_us_;
    }

    // FIXME: should be harp time.
    inline bool time_to_update()
    {return int32_t(time_us_32() - next_update_time_us) >= 0;}

    inline bool requires_update()
    {
        if (count == 0 || cycles != count)
            return true;
        return false;
    }

};
#endif // SCHEDULED_PWM_H
