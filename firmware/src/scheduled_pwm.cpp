#include <scheduled_pwm.h>

void ScheduledPWM::ScheduledPWM(uint32_t t_delay_us, uint32_t t_rise_us,
                                uint32_t t_fall_us, uint32_t t_period_us,
                                uint32_t pin_mask, uint32_t count, bool invert)
{
    if (invert)
    {
        // use SIO pin inversion on the pin mask. gpio_outover
    }
}


void ScheduledPWM::update()
{
    if (!time_to_update())
        return;
    update_state_t next_state{state};
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
    state = next_state;
}

