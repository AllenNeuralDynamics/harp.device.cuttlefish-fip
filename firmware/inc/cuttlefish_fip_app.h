#ifndef CUTTLEFISH_FIP_APP_H
#define CUTTLEFISH_FIP_APP_H
#include <pico/stdlib.h>
#include <cstring>
#include <config.h>
#include <harp_message.h>
#include <harp_core.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#include <etl/vector.h>
#include <schedule_ctrl_queues.h>
#include <core1_main.h>
#include <pico/multicore.h>
#include <laser_fip_task.h>
#ifdef DEBUG
    #include <stdio.h>
    #include <cstdio> // for printf
#endif

// Setup for Harp App
inline constexpr uint8_t REG_COUNT = 13;

extern etl::vector<LaserFIPTask, 8> laser_fip_tasks;
extern PWMScheduler pwm_schedule;
extern RegSpecs app_reg_specs[reg_count];
extern RegFnPair reg_handler_fns[reg_count];
extern HarpCApp& app;

#pragma pack(push, 1)
struct app_regs_t
{
    uint8_t EnableTaskSchedule;
    LaserFIPTaskSettings AddLaserTask;
    uint8_t RemoveLaserTask;
    uint8_t RemoveAllLaserTasks;
    uint8_t LaserTaskCount;
    LaserFIPTaskSettings ReconfigureLaserTask0;
    LaserFIPTaskSettings ReconfigureLaserTask1;
    LaserFIPTaskSettings ReconfigureLaserTask2;
    LaserFIPTaskSettings ReconfigureLaserTask3;
    LaserFIPTaskSettings ReconfigureLaserTask4;
    LaserFIPTaskSettings ReconfigureLaserTask5;
    LaserFIPTaskSettings ReconfigureLaserTask6;
    LaserFIPTaskSettings ReconfigureLaserTask7;
    // More app "registers" here.
};
#pragma pack(pop)

extern app_regs_t app_regs;

// TODO: handler functions!

/**
 * \brief read whether the laser task schedule is enabled or not.
 */
void read_enable_task_schedule(uint8_t reg_address);
void read_add_laser_task(uint8_t address); // stub?
void read_remove_laser_task(uint8_t address); // stub?
void read_remove_all_laser_takss(uint8_t address); // stub?
void read_laser_task_count(uint8_t address);
void read_reconfigure_laser_task(uint8_t address);

void write_enable_task_schedule(msg_t& msg);
void write_add_laser_task(msg_t& msg);
void write_remove_laser_task(msg_t& msg);
void write_remove_all_laser_taks(msg_t& msg);
void write_laser_task_count(msg_t& msg);
void write_reconfigure_laser_task(msg_t& msg);

/**
 * \brief update the app state. Called in a loop.
 */
void update_app();

/**
 * \brief reset the app.
 */
void reset_app();

#endif // CUTTLEFISH_FIP_APP_H
