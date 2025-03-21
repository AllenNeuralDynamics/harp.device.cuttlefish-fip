#ifndef PULSETRAIN_TASK_H
#define PULSETRAIN_TASK_H
#include <task.h>
#include <cstdint>
#include <pico/stdlib.h>
#include <hardware/gpio.h>
#include <cstdio> // for printf


/**
 * \brief datapoint representing the pin state and the relative time
 *  when that state takes place.
 */
struct pulse_event_t : event_t
{
    bool pin_state;     /// The state of the GPIO pin.

    // Set a default constructor with unused func ptr.
    pulse_event_t(bool pin_state, uint32_t us, void(*func_ptr)() = nullptr)
    : event_t(us, func_ptr), pin_state(pin_state){}
    //: pin_state{pin_state}, us{us}, func_ptr{func_ptr}{}
};

/**
 * \brief a periodic sequence of pulses task
 */
class PulseTrainTask: public Task
{
public:

    friend class TaskScheduler;
/**
 * \brief constructor.
 * \param pulse_events reference to an array of pulse event ptrs indicating the
 *  state of the pins in \p pin_mask and the relative time (in microseconds)
 *  when that state takes place.
 * \param pulse_event_count number of pulse events. Must be at least 2.
 * \param period_us the period of the task (a duration) in microseconds.
 *  Must be greater-than or equal-to the time value in the last pulse event.
 * \param pin_mask the GPIO pins that this task will apply to.
 * \param count number of iterations or 0 to loop forever unless stopped.
 */
    PulseTrainTask(pulse_event_t** pulse_events, size_t event_count,
            uint32_t period_us, uint32_t pin_mask, size_t count = 0);

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
    virtual inline void stop()
    {gpio_put_masked(pin_mask_, 0);}

    virtual void reset();

    void start();

protected:

/**
 * \brief Apply change in update() loop and call any attached observer function
 *  if defined.
 * \note overrideable by child classes.
 */
    inline void update_outputs()
    {
        uint32_t mask_value = pulse_events_[event_index_]->pin_state ? 0xFFFFFFFF: 0;
        gpio_put_masked(pin_mask_, mask_value);
        // Apply function ptr.
        if (pulse_events_[event_index_]->func_ptr != nullptr)
            pulse_events_[event_index_]->func_ptr();
    }


    pulse_event_t** pulse_events_;

    uint32_t pin_mask_; /// active channels.
    update_state_t state_;

};
#endif // PULSETRAIN_TASK_H
