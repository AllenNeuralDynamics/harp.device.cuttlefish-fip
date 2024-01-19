#ifndef PWM_TASK_H
#define PWM_TASK_H
#include <stdint.h>
//#include <pico/stdlib.h>
#include <hardware/gpio.h>
#include <hardware/irq.h>
#include <etl/priority_queue.h>
#ifdef DEBUG
#include <cstdio> // for printf
#endif

/**
 * \brief container for bookkeeping update times/states of a PWM task.
 */
class PWMTask
{
public:
    PWMTask(uint32_t t_delay_us, uint32_t t_rise_us,
                 uint32_t t_fall_us, uint32_t t_period_us,
                 uint8_t pin, uint32_t count = 0,
                 bool invert = false);

    ~PWMTask();

    enum update_state_t: uint8_t
    {
        LOW = 0,
        HIGH = 1
    }
    uint32_t delay_us_;
    uint32_t on_time_us_; // effectively duty_cycle
    uint32_t period_us_;

    uint32_t pin_mask_; // active channels.

    // FIXME: we need a comparison operator to sort which PWMTask instance
    //  should update next..

    void update(bool force = false, bool skip_output_action = false);

/**
 * \brief public wrapper for the next absolute time this instance must update.
 */
    const uint64_t& next_update_time;
/**
 * \brief public wrapper for the current state.
 */
    const uint8_t& state;

    inline void start()
    {
        start_time_us_ = time_us_64(); // TODO: should access pico time.
        reset();
    }

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
    update_state_t state_;
    uint64_t start_time_us_;
    uint64_t next_update_time_us_; // relative to the start time.

    inline void reset()
    {   state = LOW;
        next_update_time_us_ = start_time_us_ + delay_us_;
    }

/**
 * \brief true if an update is due/overdue.
 */
    inline bool time_to_update() // FIXME: should be harp time.
    {return int32_t(time_us_64() - next_update_time_us) >= 0;}

/**
 * \brief true if update() must be called again in the future.
 */
    inline bool requires_future_update()
    {
        if (count == 0 || cycles != count)
            return true;
        return false;
    }

};
#endif // PWM_TASK_H
