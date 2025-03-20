#include <pwm_task.h>


PWMTask::PWMTask(pwm_events_t (&pwm_events)[],
    size_t pulse_event_count, uint32_t period_us, uint32_t pwm_pin,
    uint32_t count)
: pulse_events_{pwm_events}, pulse_event_count_{pwm_event_count},
 period_us_{period_us}, pin_mask_{pin_mask}, count_{count},
 pwm_{pwm_pin}
{
    reset();
}

// TODO: use the base class update function by teasing out base class functionality
// into a pure virtual class.

void PulseTrainTask::reset()
{
    stop();
    pwm._enable_output();
    pulse_index_ = 0;
    loops_ = 0;
}

