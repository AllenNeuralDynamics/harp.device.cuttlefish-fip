#ifndef FIP_SCHEDULE_H
#define FIP_SCHEDULE_H

#include <hardware/timer.h>
#include <laser_fip_task.h>
#include <config.h>
#include <etl/vector.h>


inline constexpr uint32_t PWM_470_PIN = 1;
inline constexpr uint32_t PWM_415_PIN = 2;
inline constexpr uint32_t PWM_565_PIN = 3;

inline constexpr uint32_t CAM_R_PIN = 4;
inline constexpr uint32_t CAM_G_PIN = 5;

inline constexpr uint32_t CAM_EXPOSURE_TIME = 15350;
inline constexpr uint32_t DELTA1 = CAM_EXPOSURE_TIME;
inline constexpr uint32_t DELTA2 = 666;
inline constexpr uint32_t DELTA3 = 600;
inline constexpr uint32_t DELTA4 = 50;


inline constexpr uint32_t ENABLED_DIGITAL_OUTPUTS = 0xFFFFFFFF;


extern etl::vector<LaserFIPTask, MAX_TASK_COUNT> laser_fip_task;

extern bool enabled;


/**
 * \brief loop forever.
 */
void run();

void update_tasks_state();

inline void update_fip_tasks();

void run_sequence();

void run_exposure(LaserFIPTask& laser_fip_task);

uint64_t time_us_64_unsafe();
uint32_t time_us_32_fast();

void sleep_us(uint32_t us);


void setup_fip_schedule();

void run();

void run_sequence(uint32_t output_state, uint64_t time_us);

void run_exposure(PWM& laser, uint32_t camera_mask);

void push_harp_msg(uint32_t output_state, uint64_t time_us);


#endif //FIP_SCHEDULE_H
