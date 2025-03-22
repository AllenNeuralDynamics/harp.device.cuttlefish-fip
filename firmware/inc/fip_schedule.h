#ifndef FIP_SCHEDULE_H
#define FIP_SCHEDULE_H

#include <hardware/timer.h>

inline constexpr uint32_t CAM_EXPOSURE_TIME = 15350;
inline constexpr uint32_t DELTA2 = 666;
inline constexpr uint32_t DELTA3 = 600;
inline constexpr uint32_t DELTA4 = 50;

inline constexpr uint32_t ENABLED_DIGITAL_OUTPUTS = 0xFFFFFFFF;


PWM pwm470;
PWM pwm415;
PWM pwm565;

/**
 * \brief run sequence
 */
void run();

void run_sequence();

void run_exposure();


void sleep_us(uint32_t us);


#endif //FIP_SCHEDULE_H
