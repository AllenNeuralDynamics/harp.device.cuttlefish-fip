#include <pwm_task.h>

PWMTask::PWMTask(uint32_t t_delay_us, uint32_t t_on_us, uint32_t t_period_us,
                 uint8_t gpio_pin, uint32_t count, bool invert)
: delay_us_{t_delay_us}, on_time_us_{t_on_us}, period_us_{t_period_us},
  pin_{gpio_pin}
{
    // Initialize this GPIO pin.
    gpio_init(pin_);
    if (invert)
        gpio_set_outover(pin_, GPIO_OVERRIDE_INVERT);
    gpio_set_dir(pin_, true); // configure as output.
    gpio_put(pin_, 0);

    // TODO: define pin mask.
}

PWMTask::~PWMTask()
{}

void PWMTask::update(bool force, bool skip_output_action)
{
    if ((!force) && (!time_to_update()))
        return;
    update_state_t next_state{state_};
    switch (state_)
    {
        case HIGH:
            next_state = LOW;
            next_update_time_us_ += period_us_ - on_time_us_;
            break;
        case LOW:
            next_state = HIGH;
            next_update_time_us_ += on_time_us_;
            break;
    }
    if (state_ == HIGH && next_state == LOW)
        cycles_ += 1;
    state_ = next_state;

    // Update outputs.
    // Update the gpio state here.
    if (skip_output_action)
        return;
    // Apply the GPIO state change here.
    gpio_put(pin_, (1u << state_));
}

