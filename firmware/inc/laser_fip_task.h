#ifndef LASER_FIP_TASK_H
#define LASER_FIP_TASK_H

#include <pwm.h>


struct LaserFIPTaskSettings
{
    size_t pwm_pin;
    float pwm_duty_cycle;
    float pwm_frequency_hz;

    uint32_t output_mask;

    uint8_t events; // if true, rising-edge event msgs are enabled.
    uint8_t mute;   // if true, the task will take place, but all outputs will stay LOW.

    uint32_t delta1_us;
    uint32_t delta2_us;
    uint32_t delta3_us;
    uint32_t delta4_us;
};


class  LaserFIPTask
{
public:


/**
 * \brief constructor.
 */
    LaserFIPTask(size_t pwm_pin, float pwm_duty_cycle, float pwm_frequency_hz,
                 uint32_t output_mask, bool enable_events, bool mute_output,
                 uint32_t delta1_us, uint32_t delta2_us, uint32_t delta3_us,
                 uint32_t delta4_us);

     ~LaserFIPTask();

    LaserFIPTaskSettings settings_;

    PWM laser_;


    inline void set_output()
    {
        if (settings_.mute)
            return;
        gpio_put_masked(settings_.output_mask, 0xFFFFFFFF);
    }

    inline void clear_output()
    {gpio_put_masked(settings_.output_mask, 0);}

    inline uint32_t output_mask()
    {
        if (settings_.mute)
            return 0;
        return settings_.output_mask;
    }

};
#endif // LASER_FIP_TASK_H
