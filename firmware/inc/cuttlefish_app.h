#ifndef CUTTLEFISH_APP_H
#define CUTTLEFISH_APP_H
#include <pico/stdlib.h>
#include <cstring>
#include <config.h>
#include <harp_message.h>
#include <harp_core.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#include <etl/vector.h>
#include <pwm_scheduler.h>
#include <pwm_task.h>
#include <schedule_ctrl_queues.h>
#include <core1_main.h>
#include <pico/multicore.h>
#ifdef DEBUG
    #include <stdio.h>
    #include <cstdio> // for printf
#endif

// Setup for Harp App
const size_t reg_count = 10;

extern uint8_t pwm_task_mask;
extern PWMScheduler pwm_schedule;
extern RegSpecs app_reg_specs[reg_count];
extern RegFnPair reg_handler_fns[reg_count];
extern HarpCApp& app;

#pragma pack(push, 1)
struct app_regs_t
{
    volatile uint8_t port_dir; // 1 = output; 0 = input.
    volatile uint8_t port_state; // current gpio state. Readable and writeable.
                                 // Issues event if port state has changed for
                                 // port channels marked as inputs.
                                 // Writing to pins shared with a task
                                 // does not change their state.
    // More app "registers" here.
};
#pragma pack(pop)

extern app_regs_t app_regs;


void reset_schedule();

/**
 * \brief declare pins inputs or outputs.
 * \warning this status can be overwritten if a pin is later assigned to a Task
 */
void write_port_dir(msg_t& msg);

/**
 * \brief read the 8-channel port simultaneousy
 */
void read_port_state(uint8_t reg_address);

/**
 * \brief write to the 8-channel port as a group. (Values for pins specified
 *  as inputs will be ignored.)
 */
void write_port_state(msg_t& msg);

/**
 * \brief update the app state. Called in a loop.
 */
void update_app_state();

/**
 * \brief reset the app.
 */
void reset_app();

#endif // CUTTLEFISH_APP_H
