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
    PWMTask(uint32_t t_delay_us, uint32_t t_on_us, uint32_t t_period_us,
            uint32_t pin_mask, uint32_t count = 0, bool invert = false);

    ~PWMTask();

/**
 * \brief comparison operator for scheduling.
 */
    friend bool operator<(const PWMTask& lhs, const PWMTask& rhs)
    {return int32_t(rhs.next_update_time_us_ - lhs.next_update_time_us_) > 0;}

    enum update_state_t: uint8_t
    {
        LOW = 0,
        HIGH = 1,
        DONE = 2,
        DELAY = 3,
    };

/**
 * \brief should be called in a loop.
 * \param[force] if true, the pwm state change will forcibly iterate, which is
 *  useful if time/actions are managed by an external scheduler.
 * \param[skip_output_action] if true, this class will not manipulate the GPIO
 *  pins in the pin mask
 */
    void update(bool force = false, bool skip_output_action = false);

/**
 * \brief read-only public wrapper for the next absolute time that this
 *  instance must update.
 */
    const inline uint32_t next_update_time_us()
    {return next_update_time_us_;}

/**
 * \brief stop outputs.
 */
    inline void stop()
    {gpio_put_masked(pin_mask_, 0);}

    void reset(bool skip_output_action = false);

    inline void start(bool skip_output_action = false)
    {
        reset(skip_output_action);
        set_time_started(timer_hw->timerawl);
    }

    inline void set_time_started(uint32_t start_time_us)
    {
        start_time_us_ = start_time_us;
        next_update_time_us_ = start_time_us_ + delay_us_;
        if (starting_state() == HIGH)
            next_update_time_us_ += on_time_us_;
    }

/**
 * \brief true if an update is due/overdue.
 */
    inline bool time_to_update() // FIXME: should be harp time.
    {return int32_t(timer_hw->timerawl - next_update_time_us_) >= 0;}

/**
 * \brief true if update() must be called again in the future.
 */
    inline bool requires_future_update()
    {return state_ != DONE;}

    inline update_state_t starting_state()
    {return (delay_us_ == 0)? HIGH : DELAY;}

private:
    friend class PWMScheduler;

    uint32_t delay_us_; /// pulse train delay (phase offset) in microseconds.
    uint32_t on_time_us_; /// pulse train duty cycle in microseconds
    uint32_t period_us_; /// pulse train period in microseconds

    uint32_t pin_mask_; /// active channels.
    update_state_t state_; /// current state of pulse waveform.
    uint32_t count_; /// How many pulses to issue.
                     ///  0: pulse forever. >0: execute N times.
    bool invert_;   /// Whether the waveform is inverted. Used externally.
    uint32_t loops_; /// how many iterations of the state machine we have been
                     ///    through.
    uint32_t cycles_; /// how many times we have pulsed.
    uint32_t start_time_us_; /// What (32-bit) time the pulse started.

/**
 * \brief absolute time that the state machine needs to update.
 */
    uint32_t next_update_time_us_;
};
#endif // PWM_TASK_H
