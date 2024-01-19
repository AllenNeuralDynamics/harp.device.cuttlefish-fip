#include <pwm_task.h>

void PWMTask::PWMTask(uint32_t t_delay_us, uint32_t t_rise_us,
                                uint32_t t_fall_us, uint32_t t_period_us,
                                uint8_t pin, uint32_t count, bool invert)
:next_update_time_us{next_update_time_us_}, state{state_}
{
    // Initialize this GPIO pin.
    gpio_init(pin);
    if (invert)
        gpio_set_outover(pin, GPIO_OVERRIDE_INVERT);
    gpio_set_dir(pin, true); // configure as output.
    gpio_put(pin, 0);

    // TODO: define pin mask.
}


void PWMTask::update(bool force, bool skip_output_action)
{
    if ((!force) && (!time_to_update()))
        return;
    update_state_t next_state{state_};
    switch (state)
    {
        case HIGH:
            next_state ^= 1; // LOW
            next_update_time_us = t_fall_us;
            break;
        case LOW
            next_state ^= 1; // HIGH
            next_update_time_us = period_us - t_high_us;
            break;
    }
    if (state == HIGH && next_state == LOW)
        cycles += 1;
    state_ = next_state;

    // Update outputs.
    // Update the gpio state here.
    if (skip_output_action)
        return;
    // Apply the GPIO state change here.
    gpio_put(pin, (1u << state_));
}

