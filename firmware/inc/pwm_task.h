#ifndef PWM_TASK_H
#define PWM_TASK_H
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/gpio.h>
#ifdef DEBUG
    #include <cstdio> // for printf
#endif

/**
 * \brief container for bookkeeping update times/states of a PWM task.
 */
class PWMTask
{
public:
    // FIXME: pin should be a pin mask since this PWMTask could apply to multiple pins.
    PWMTask(uint32_t t_delay_us, uint32_t t_on_us, uint32_t t_period_us,
            uint8_t gpio_pin, uint32_t count = 0, bool invert = false);

    ~PWMTask();

/**
 * \brief comparison operator for scheduling.
 * \note <=, >, >= derived from this operator in etl comparison implementation.
 */
    friend bool operator<(const PWMTask& lhs, const PWMTask& rhs)
    {return int32_t(rhs.next_update_time_us_ - lhs.next_update_time_us_) > 0;}

    enum update_state_t: uint8_t
    {
        LOW = 0,
        HIGH = 1
    };
    uint32_t delay_us_;
    uint32_t on_time_us_; // effectively duty_cycle
    uint32_t period_us_;

    uint32_t pin_; // active channels.

    // TODO: make protected or use friend class.
    update_state_t state_;

    void update(bool force = false, bool skip_output_action = false);

/**
 * \brief read-only public wrapper for the next absolute time that this
 *  instance must update.
 */
    const inline uint64_t next_update_time_us()
    {return next_update_time_us_;}

/**
 * \brief read-only public wrapper for the gpio pin.
 */
    const inline uint8_t pin(){return pin_;}

    // TODO: should access Harp time.
    inline void start()
    {start_at_time(time_us_64());}

/**
 * \brief start the PWM state machine but override its start time to
 *  schedule it in the future.
 */
    inline void start_at_time(uint64_t start_time_us)
    {
        start_time_us_ = start_time_us;
        reset();
    }

/**
 * \brief true if an update is due/overdue.
 */
    inline bool time_to_update() // FIXME: should be harp time.
    {return int32_t(time_us_64() - next_update_time_us_) >= 0;}

/**
 * \brief true if update() must be called again in the future.
 */
    inline bool requires_future_update()
    {
        if (count_ == 0 || cycles_ != count_)
            return true;
        return false;
    }

private:
    uint32_t count_; // N==0: pulse forever. N>0: execute N times.
    uint32_t cycles_; // how many times we have pulsed.
    uint64_t start_time_us_;
/**
 * \brief absolute time that the state machine needs to update.
 */
    uint64_t next_update_time_us_;

    inline void reset()
    {
        cycles_ = 0;
        gpio_put(pin_, 0);
        state_ = LOW;
        next_update_time_us_ = start_time_us_ + delay_us_;
    }
};
#endif // PWM_TASK_H
