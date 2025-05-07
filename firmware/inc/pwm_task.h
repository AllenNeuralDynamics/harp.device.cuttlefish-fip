#ifndef PWM_TASK_H
#define PWM_TASK_H
#include <task.h>
#include <pwm.h>
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/gpio.h>
#include <cstdio> // for printf

/**
 * \brief datapoint representing the pwm pin state and the relative time
 *  when that state takes place.
 */
struct pwm_event_t : event_t
{
    float duty_cycle;
    float frequency_hz;

    // Set a default constructor with unused func ptr.
    pwm_event_t(float duty_cycle, float frequency_hz, uint32_t us,
                 void(*func_ptr)() = nullptr)
    : event_t(us, func_ptr),
      duty_cycle(duty_cycle), frequency_hz(frequency_hz){}
};

/**
 * \brief a periodic sequence of pwm outputs task
 */
class PWMTask: public Task
{
public:
/**
 * \brief constructor.
 * \param pwm_events
 * \param pulse_event_count number of pulse events. Must be at least 2.
 * \param period_us the period of the task (a duration) in microseconds.
 *  Must be greater-than or equal-to the time value in the last pulse event.
 * \param pin_mask the GPIO pins that this task will apply to.
 * \param count number of iterations or 0 to loop forever unless stopped.
 */
    PWMTask(pwm_event_t** pwm_events, size_t event_count,
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

protected:

/**
 * \brief Apply change in update() loop and call any attached observer function
 *  if defined.
 * \note overrideable by child classes.
 */
    virtual inline void update_outputs() override
    {
        float& duty_cycle = pwm_events_[event_index_]->duty_cycle;
        float& frequency_hz = pwm_events_[event_index_]->frequency_hz;
        pwm_.set_frequency(frequency_hz);
        pwm_.set_duty_cycle(duty_cycle);
        // Apply function ptr.
        if (pwm_events_[event_index_]->func_ptr != nullptr)
            pwm_events_[event_index_]->func_ptr();
    }

protected:
    pwm_event_t** pwm_events_;

    PWM pwm_;

};
#endif // PWM_TASK_H
