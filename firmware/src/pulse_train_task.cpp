#include <pulse_train_task.h>


PulseTrainTask::PulseTrainTask(pulse_event_t** pulse_events,
    size_t event_count, uint32_t period_us, uint32_t pin_mask,
    size_t count)
: Task((event_t**)pulse_events, event_count, period_us, count),
  pulse_events_{pulse_events}, pin_mask_{pin_mask}
{
    reset();
}

void PulseTrainTask::update()
{Task::update();}

void PulseTrainTask::reset()
{
    stop();
    gpio_init_mask(pin_mask_);
    gpio_set_dir_out_masked(pin_mask_);
    Task::reset();
}

void PulseTrainTask::start()
{Task::start();}

