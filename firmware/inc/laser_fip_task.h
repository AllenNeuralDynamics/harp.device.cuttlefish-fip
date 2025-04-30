#ifndef LASER_FIP_TASK_H
#define LASER_FIP_TASK_H
#include <pwm.h>

#pragma pack(push, 1)
struct LaserFIPTaskSettings
{
    uint32_t pwm_pin_bit; // one-hot encoded pwm pin.
    float pwm_duty_cycle;
    float pwm_frequency_hz;

    uint32_t output_mask;

    uint8_t events; // if true, rising-edge event msgs are enabled.
    uint8_t mute;   // if true, the task will take place, but all outputs will stay LOW.

    uint32_t delta1_us;
    uint32_t delta2_us;
    uint32_t delta3_us;
    uint32_t delta4_us;


/**
 * \brief convert single pin set in a pin mask to its corresponding integer
 *  representation.
 */
    static int32_t onehot_to_pin(uint32_t bitmask)
    {
        size_t offset = 0;
        for (size_t index = 0; index < 32; ++ index)
        {
            if ((bitmask & 1u) )
                return offset;
            else
            {
                bitmask >>= 1;
                ++offset;
            }
        }
        return -1;
    }
};
#pragma pack(pop)


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

/**
 * \brief apply the settings specified.
 */
    inline void apply_settings(LaserFIPTaskSettings& settings)
    {settings_ = settings;} // Use default assignment operator.

    inline void set_output()
    {gpio_put_masked(output_mask(), 0xFFFFFFFF);}

    inline void clear_output()
    {gpio_put_masked(settings_.output_mask, 0);}

    inline uint32_t output_mask()
    {
        if (settings_.mute)
            return 0;
        return settings_.output_mask;
    }

    LaserFIPTaskSettings settings_;
    PWM laser_;
};
#endif // LASER_FIP_TASK_H
