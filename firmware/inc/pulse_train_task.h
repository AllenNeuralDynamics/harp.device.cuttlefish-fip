#ifndef PULSETRAIN_TASK_H
#define PULSETRAIN_TASK_H
#include <task.h>
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/gpio.h>
#ifdef DEBUG
    #include <cstdio> // for printf
#endif

/**
 * \brief datapoint representing the pin state and the relative time
 *  when that state takes place.
 */
struct pulse_events_t
{
    bool pin_state;     /// The state of the GPIO pin.
    uint32_t us;        /// The time at which the pin_state takes place.
    void(*func_ptr)();  /// any callback fn for implementing observer pattern.


    // Set a default constructor with unused func ptr.
    pulse_events_t(bool pin_state, uint32_t us, void(*func_ptr)() = nullptr)
    : pin_state{pin_state}, us{us}, func_ptr{func_ptr}{}
};

/**
 * \brief a periodic sequence of pulses task
 */
class PulseTrainTask: public Task
{
public:
/**
 * \brief constructor.
 * \param pulse_events an array of pulse events indicating the state of the
 *  pins in \p pin_mask and the relative time (in microseconds) when that
 *  state takes place.
 * \param pulse_event_count number of pulse events. Must be at least 2.
 * \param period_us the period of the task (a duration) in microseconds.
 *  Must be greater-than or equal-to the time value in the last pulse event.
 * \param pin_mask the GPIO pins that this task will apply to.
 * \param count number of iterations or 0 to loop forever unless stopped.
 */
    PulseTrainTask(pulse_events_t (&pulse_events)[], size_t pulse_event_count,
            uint32_t period_us, uint32_t pin_mask, uint32_t count = 0)
    : pulse_events_{pulse_events}, pulse_event_count_{pulse_event_count},
      pin_mask_{pin_mask}, count_{count} {}

    ~PulseTrainTask();

    enum update_state_t: uint8_t
    {
        APPLYING_PULSE_EVENTS = 0,
        DONE = 1,
    };

/**
 * \brief should be called in a loop.
 */
    void update();

/**
 * \brief stop outputs.
 */
    inline void stop()
    {gpio_put_masked(pin_mask_, 0);}

    void reset();

    void start();

/**
 * \brief true if update() must be called again in the future.
 */
    inline bool requires_future_update() override
    {return state_ != DONE;}

private:
    friend class TaskScheduler;

    pulse_events_t (&pulse_events_)[];
    size_t pulse_event_count_;

    uint32_t period_;
    uint32_t pin_mask_; /// active channels.
    size_t count_; /// How many pulses to issue.
                     ///  0: pulse forever. >0: execute N times.
    uint32_t loops_; /// how many iterations of the state machine we have been
                     ///    through.
    update_state_t state_;
    uint32_t start_time_us_; /// What (32-bit) time the pulse started.

};
#endif // PULSETRAIN_TASK_H
