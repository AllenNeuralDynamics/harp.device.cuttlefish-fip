#include <fip_schedule.h>

void sleep_us(uin32_t us)
{
    uint32_t start_time_us = timer_hw->timerawl;
    while (int32_t(timer_hw->timerawl - start_time_us) < us)
        asm volatile("nop");
}

void run()
{
    // do a sequence.
    while (true)
    {
        run_sequence();
        if (stop_received())
            return;
    }
}

void run_sequence()
{
    run_exposure(pwm470, (1u << CAM_G_OFFSET));
    run_exposure(pwm415, (1u << CAM_G_OFFSET));
    run_exposure(pwm515, (1u << CAM_R_OFFSET));
}

// TODO: inline this.
void run_exposure(PWM& pwm, uint32_t camera_mask)
{
    // TODO: tweak delays to account for elapsed time to trigger signals.
    laser.enable_output();
    // Send pin state w/ pwm rising edge.
    push_harp_msg((1u << laser.pwm_pin_), time_us_64_unsafe());
    sleep_us(DELTA3);
    gpio_put_masked((camera_mask & ENABLED_DIGITAL_OUTPUTS), camera_mask);
    // Send pin state w/ CAM_G rising edge.
    push_harp_msg(
        ((1u << laser.pwm_pin_) | (camera_mask & ENABLED_DIGITAL_OUTPUTS)),
        time_us_64_unsafe());
    sleep(CAM_EXPOSURE_TIME);
    gpio_put_masked((camera_mask & ENABLED_DIGITAL_OUTPUTS), 0);
    sleep_us(DELTA4)
    laser.disable_output();
    sleep_us(DELTA2);
}

