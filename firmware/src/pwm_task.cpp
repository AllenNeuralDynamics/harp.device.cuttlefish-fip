#include <pwm_task.h>


PWMTask::PWMTask(pwm_event_t** pwm_events, size_t event_count,
    uint32_t period_us, uint32_t pwm_pin, uint32_t count)
: Task((event_t**)pwm_events, event_count, period_us, count),
 pwm_events_(pwm_events), pwm_{pwm_pin}
{
    reset();
}

void PWMTask::update()
{Task::update();}

void PWMTask::reset()
{
    stop();
    pwm_.enable_output();
    Task::reset();
}

void PWMTask::start()
{Task::start();}
