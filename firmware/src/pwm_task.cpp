#include <pwm_task.h>

PWMTask::PWMTask(uint32_t t_delay_us, uint32_t t_on_us, uint32_t t_period_us,
                 uint32_t pin_mask, uint32_t count, bool invert)
: delay_us_{t_delay_us}, on_time_us_{t_on_us}, period_us_{t_period_us},
  pin_mask_{pin_mask}, count_{count_}, cycles_{0}, start_time_us_{0},
  next_update_time_us_{0}
{
    // Initialize this GPIO pin.
    gpio_init_mask(pin_mask_);
    // Invert.
    if (invert)
    {
        for (uint8_t i = 0; i < 30; ++i)
        {
            if (0x00000001 & (pin_mask_ >> i))
                gpio_set_outover(i, GPIO_OVERRIDE_INVERT);
        }
    }
    gpio_set_dir_masked(pin_mask_, pin_mask_); // configure as output.
    gpio_put_masked(pin_mask_, 0);

    // set starting state.
    state_ = (delay_us_ == 0)? HIGH : LOW;
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
    uint32_t pin_state = (state_ == HIGH)? pin_mask_: 0;
    gpio_put_masked(pin_mask_, pin_state);
}

