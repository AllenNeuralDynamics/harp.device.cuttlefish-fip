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

// Create device name array.
extern const uint16_t who_am_i;
extern const uint8_t hw_version_major;
extern const uint8_t hw_version_minor;
extern const uint8_t assembly_version;
extern const uint8_t harp_version_major;
extern const uint8_t harp_version_minor;
extern const uint8_t fw_version_major;
extern const uint8_t fw_version_minor;
extern const uint16_t serial_number;

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
    volatile uint8_t pwm_task[sizeof(pwm_task_specs_t)]; // {offset_us (U32),
                                                         //  start_time_us (U32),
                                                         //  stop_time_us (U32),
                                                         //  port_mask (U8),
                                                         //  cycles (U32),
                                                         //  invert (U8)}
                                                         // This register reads
                                                         // all zeros.
    volatile uint8_t arm_ext_trigger; // which port input pin(s) cause the
                                      // schedule to trigger.
                                      // Arm a hardware trigger.
                                      // [0] = 1: TTL pin 0 starts the trigger.
                                      // [1] = 1: TTL pin 1 starts the trigger.
    volatile uint8_t ext_trigger_edge; // value of the gpio pin(s) that cause
                                       // the schedule to start. Example:
                                       // [0]: set external TTL pin 0 trigger
                                       //      type to rising edge (1) or
                                       //      falling edge (0).
                                       // [1]: set external TTL pin 1 trigger
                                       //      type to rising edge (1) or
                                       //      falling edge (0).
                                       // ...
                                       // Resets to rising edge.
    volatile uint8_t arm_ext_untrigger; // 
    volatile uint8_t ext_untrigger_edge;  // 
    volatile uint8_t sw_trigger;    // Writing nonzero value to this register
                                    // starts the schedule.
    volatile uint8_t sw_untrigger;

    volatile uint8_t schedule_ctrl; // Apply/read various settings. Reads as 0.
                                    // [0] : Stop and clear all configured
                                    //       pwm_tasks.
                                    // [1] : dump 1 event message per configured
                                    //       pwm task from the pwm_task register
                                    //       followed by a
                                    //       write message from this register.
                                    // [4:7]: number of configured tasks.

    // More app "registers" here.
};
#pragma pack(pop)

extern app_regs_t app_regs;


inline void reset_schedule()
{
    multicore_reset_core1(); // Kill any actively running schedule.
    gpio_put_masked((0x000000FF << PORT_BASE), 0); // Write all GPIOs to 0.
    multicore_launch_core1(core1_main);
}

/**
 * \brief declare pins inputs or outputs.
 * \warning this status can be overwritten if a pin is later assigned to a PWMTask
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
 * \brief Create a PWMTask.
 */
void write_pwm_task(msg_t& msg);

void write_arm_ext_trigger(msg_t& msg);

void write_ext_trigger_edge(msg_t& msg);

void write_arm_ext_untrigger(msg_t& msg);

void write_ext_untrigger_edge(msg_t& msg);

void write_sw_trigger(msg_t& msg);

void write_sw_untrigger(msg_t& msg);

void write_schedule_ctrl(msg_t& msg);

/**
 * \brief update the app state. Called in a loop.
 */
void update_app_state();

/**
 * \brief reset the app.
 */
void reset_app();

#endif // CUTTLEFISH_APP_H
