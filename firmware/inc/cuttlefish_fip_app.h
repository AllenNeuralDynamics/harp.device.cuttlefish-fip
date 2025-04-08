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
#include <pico/multicore.h>
#include <laser_fip_task.h>
#ifdef DEBUG
    #include <stdio.h>
    #include <cstdio> // for printf
#endif

// Setup for Harp App
inline constexpr uint8_t REG_COUNT = 14;
inline constexpr uint8_t LASER_BASE_ADDRESS = APP_REG_START_ADDRESS + 5;

extern etl::vector<LaserFIPTask, MAX_TASK_COUNT> fip_tasks;
extern RegSpecs app_reg_specs[REG_COUNT];
extern RegFnPair reg_handler_fns[REG_COUNT];
extern HarpCApp& app;

#pragma pack(push, 1)
struct app_regs_t
{
    uint8_t EnableTaskSchedule;
    LaserFIPTaskSettings AddLaserTask;
    uint8_t RemoveLaserTask;
    uint8_t RemoveAllLaserTasks;
    uint8_t LaserTaskCount;
    uint8_t RisingEdgeEvent;
    LaserFIPTaskSettings ReconfigureLaserTask[MAX_TASK_COUNT];
    // More app "registers" here.
};
#pragma pack(pop)

enum AppRegNum
{
    EnableTaskSchedule = 32,
    AddLaserTask = 33,
    RemoveLaserTask = 34,
    RemoveAllLaserTasks = 35,
    LaserTaskCount = 36,
    RisingEdgeEvent = 37,
    ReconfigureLaserTask0 = 38,
    ReconfigureLaserTask1 = 39,
    ReconfigureLaserTask2 = 40,
    ReconfigureLaserTask3 = 41,
    ReconfigureLaserTask4 = 42,
    ReconfigureLaserTask5 = 43,
    ReconfigureLaserTask6 = 44,
    ReconfigureLaserTask7 = 45,

};

extern app_regs_t app_regs;

/**
 * \brief helper function. Get fip task index from app reg index.
 */
inline size_t get_fip_task_index(uint8_t address)
{return address - LASER_BASE_ADDRESS;}

/**
 * \brief helper function. Get fip task index from msg.
 */
inline size_t get_fip_task_index(msg_t& msg)
{return msg.header.address - LASER_BASE_ADDRESS;}

/**
 * \brief read whether the laser task schedule is enabled or not.
 */
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
