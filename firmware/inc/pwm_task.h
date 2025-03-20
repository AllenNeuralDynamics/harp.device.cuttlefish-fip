#ifndef PWM_TASK_H
#define PWM_TASK_H
#include <pulse_train_task.h>
#include <pwm.h>
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/gpio.h>
#include <cstdio> // for printf

/**
 * \brief datapoint representing the pin state and the relative time
 *  when that state takes place.
 */
struct pwm_events_t : event_t
{
    float duty_cycle;     /// The state of the GPIO pin.
    float frequency_hz;

    // Set a default constructor with unused func ptr.
    pwm_events_t(float duty_cycle, float frequency_hz, uint32_t us,
                 void(*func_ptr)() = nullptr)
    : duty_cycle{duty_cycle}, frequency_hz{frequency_hz}, us{us}, func_ptr{func_ptr}{}
};

/**
 * \brief a periodic sequence of pulses task
 */
class PWMTask: public PulseTrainTask
{
public:
/**
 * \brief constructor.
 * \param duty_cycle_events an array of pulse events indicating the state of the
 *  pins in \p pin_mask and the relative time (in microseconds) when that
 *  state takes place.
 * \param pulse_event_count number of pulse events. Must be at least 2.
 * \param period_us the period of the task (a duration) in microseconds.
 *  Must be greater-than or equal-to the time value in the last pulse event.
 * \param pin_mask the GPIO pins that this task will apply to.
 * \param count number of iterations or 0 to loop forever unless stopped.
 */
    PWMTask(pwm_events_t (&pwm_events)[], size_t pwm_event_count,
            uint32_t period_us, uint32_t pwm_pin, uint32_t count = 0);

    ~PWMTask()
    {stop();}

    enum update_state_t: uint8_t
    {
        APPLYING_PWM_EVENTS = 0,
        DONE = 1,
    };

/**
 * \brief should be called in a loop.
 */
    void update();

/**
 * \brief stop outputs.
 */
    inline void stop() override
    {pwm_.set_duty_cycle(0);}

    void reset();

    void start();

/**
 * \brief true if update() must be called again in the future.
 */
    inline bool requires_future_update() override
    {return state_ != DONE;}

protected:

/**
 * \brief Apply change in update() loop and call any attached observer function
 *  if defined.
 * \note overrideable by child classes.
 */
    virtual inline void update_outputs() override
    {
        uint32_t& duty_cycle = pulse_events_[pulse_index_].duty_cycle;
        uint32_t& frequency_hz = pulse_events_[pulse_index_].frequency_hz;
        pwm_.set_frequency(frequency_hz);
        pwm_.set_duty_cycle(duty_cycle);
        // Apply function ptr.
        if (pulse_events_[pulse_index_].func_ptr != nullptr)
            pulse_events_[pulse_index_].func_ptr();
    }

private:
    PWM pwm_;

};
#endif // PWM_TASK_H
