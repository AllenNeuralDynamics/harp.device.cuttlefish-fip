#include <laser_fip_task.h>


LaserFIPTask::LaserFIPTask(
    size_t pwm_pin, float pwm_duty_cycle, float pwm_frequency_hz,
    uint32_t output_mask, bool enable_events, bool mute_output,
    uint32_t delta1_us, uint32_t delta2_us, uint32_t delta3_us,
    uint32_t delta4_us)
:settings_{pwm_pin, pwm_duty_cycle, pwm_frequency_hz, output_mask, enable_events,
    mute_output, delta1_us, delta2_us, delta3_us, delta4_us},
 laser_(pwm_pin)
{
    // Configure outputs.
    gpio_init_mask(output_mask);
    gpio_set_dir_masked(output_mask, 0xFFFFFFFF);

    // Configure initial laser settings.
    laser_.set_duty_cycle(pwm_duty_cycle);
    laser_.set_frequency(pwm_frequency_hz);
};



LaserFIPTask::~LaserFIPTask()
{

};
