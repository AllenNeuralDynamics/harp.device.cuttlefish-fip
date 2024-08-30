#include <pwm_task.h>

PWMTask::PWMTask(uint32_t t_delay_us, uint32_t t_on_us, uint32_t t_period_us,
                 uint32_t pin_mask, uint32_t count, bool invert)
: delay_us_{t_delay_us}, on_time_us_{t_on_us}, period_us_{t_period_us},
  pin_mask_{pin_mask}, count_{count}, invert_{invert},
  loops_{0}, cycles_{0}, start_time_us_{0}, next_update_time_us_{0}
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
    reset(); // set starting state. Clear output to 0.
#if defined(DEBUG)
    printf("PWMTask Created!\r\n");
#endif
}


PWMTask::~PWMTask()
{
    // Un reserve pins.
#if defined(DEBUG)
    printf("PWMTask Destroyed!\r\n");
#endif
    gpio_put_masked(pin_mask_, 0);
    gpio_set_dir_masked(pin_mask_, 0); // Configure as input so as not to drive
                                       // signals.
}

void PWMTask::reset(bool skip_output_action)
{
    loops_ = 0;
    cycles_ = 0;
    state_ = starting_state();
    set_time_started(0); // Clear "start time" to 0 and set next update time
                         // relative to that such that sorting still works.
    if (skip_output_action)
        return;
    uint32_t pin_state = (state_ == HIGH)? pin_mask_ : 0;
    gpio_put_masked(pin_mask_, pin_state);
}

void PWMTask::update(bool force, bool skip_output_action)
{
    if ((!force) && (!time_to_update()))
        return;
    update_state_t next_state{state_};
    if ((cycles_ == count_) && (count_ > 0))
        next_state = DONE;
    else
    {
        switch (state_)
        {
            case DELAY:
                next_state = HIGH;
            case HIGH:
                next_state = LOW;
                next_update_time_us_ += period_us_ - on_time_us_;
                break;
            case LOW:
                next_state = HIGH;
                next_update_time_us_ += on_time_us_;
                break;
            case DONE:
                next_state = DONE;
            default: // Shouldn't be accessible in this section.
                next_state = DONE;
        }
    }
    loops_ += 1;
    cycles_ = loops_ >> 1; // loops/2.
    state_ = next_state;
    // Update outputs.
    // Update the gpio state here.
    if (skip_output_action)
        return;
    // Apply the GPIO state change here.
    uint32_t pin_state = (state_ == HIGH)? pin_mask_: 0;
    gpio_put_masked(pin_mask_, pin_state);
}

