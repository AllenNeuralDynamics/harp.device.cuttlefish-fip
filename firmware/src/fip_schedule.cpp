#include <fip_schedule.h>

PWM pwm470(PWM_470_PIN);
PWM pwm415(PWM_415_PIN);
PWM pwm565(PWM_565_PIN);

/// \warning: this fn should not be called inside an interrupt.
inline uint64_t time_us_64_unsafe()
{
    uint64_t time = timer_hw->timelr; // Locks time until we read TIMEHR.
    return (uint64_t(timer_hw->timehr) << 32) | time;
}

void sleep_us(uint32_t us)
{
    uint32_t start_time_us = timer_hw->timerawl;
    while (int32_t(timer_hw->timerawl - start_time_us) < us)
        asm volatile("nop");
}

bool stop_received()
{
    return false;
}

void setup_fip_schedule()
{
    // Declare outputs.
    gpio_init_mask((1u << CAM_G_PIN) | (1u << CAM_R_PIN));
    gpio_set_dir_masked((1u << CAM_G_PIN) | (1u << CAM_R_PIN), 0xFFFFFFFF);

    PWM* pwms[] = {&pwm470, &pwm415, &pwm565};
    for (auto pwm: pwms)
    {
        pwm->set_duty_cycle(0.5);
        pwm->set_frequency(10000.0);
    }
}

void run()
{
    // do a continuous sequence.
    while (true)
    {
        run_sequence();
        if (stop_received())
            return;
    }
}

void push_harp_msg(uint32_t output_state, uint64_t time_us)
{
    // TODO.
}

void run_sequence()
{
    run_exposure(pwm470, (1u << CAM_G_PIN));
    run_exposure(pwm415, (1u << CAM_G_PIN));
    run_exposure(pwm565, (1u << CAM_R_PIN));
}

inline void run_exposure(PWM& laser, uint32_t camera_mask)
{
    // TODO: tweak delays to account for elapsed time to trigger signals.
    laser.enable_output();
    // Send pin state w/ pwm rising edge.
    push_harp_msg((1u << laser.pin()), time_us_64_unsafe());
    sleep_us(DELTA3);
    gpio_put_masked((camera_mask & ENABLED_DIGITAL_OUTPUTS), 0xFFFFFFFF);
    // Send pin state w/ CAM_G rising edge.
    push_harp_msg(
        ((1u << laser.pin()) | (camera_mask & ENABLED_DIGITAL_OUTPUTS)),
        time_us_64_unsafe());
    sleep_us(CAM_EXPOSURE_TIME);
    gpio_put_masked((camera_mask & ENABLED_DIGITAL_OUTPUTS), 0);
    sleep_us(DELTA4);
    laser.disable_output();
    sleep_us(DELTA2);
}

