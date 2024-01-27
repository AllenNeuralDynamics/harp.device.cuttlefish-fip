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
const size_t reg_count = 8;

extern PWMScheduler pwm_schedule;
extern RegSpecs app_reg_specs[reg_count];
extern RegFnPair reg_handler_fns[reg_count];
extern HarpCApp& app;

// Allocated space for up to 8 tasks.
extern etl::vector<PWMTask, 8> pwm_tasks;

// Container to dereference packed binary data for instantiating a PWMTask.
#pragma pack(push, 1)
struct pwm_task_specs_t
{
    uint32_t offset_us;
    uint32_t on_time_us;
    uint32_t period_us;
    uint8_t port_mask;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct app_regs_t
{
    volatile uint8_t port_dir; // 1 = output; 0 = input.
    volatile uint8_t port_raw; // current gpio state. Readable and writeable.
    volatile uint8_t pwm_task[sizeof(pwm_task_specs_t)]; // {offset_us (U32),
                                                         //  start_time_us (U32),
                                                         //  stop_time_us (U32),
                                                         //  port_mask (U8)}
    volatile uint8_t trigger_type;  // [0]=1 : absolute time stored in
                                    //         trigger_time_us starts th
                                    //         schedule.
                                    // [1]=1 : gpio pin configured as input
                                    //         starts the schedule.
                                    // [3:2]=xx  : pin rising-edge [00],
                                    //             falling-edge [01], or
                                    //             change[11]
                                    //             starts the schedule.
                                    // [7]=1 : start the schedule now
                                    //         (i.e: software trigger).
                                    // reset value = 0.
    volatile uint8_t pin_trigger_mask; // which port pins (configured as input)
                                       // cause the schedule to trigger.
    volatile uint8_t pin_trigger_state; // value of the gpio pin(s) that caue
                                        // the schedule to start.
    volatile uint32_t time_trigger_us; // time (in "harp time" in microseconds)
                                       // when the schedule starts.
    volatile uint8_t schedule_ctrl; // [0] :  clear schedule
                                    // [1] : dump schedule
    // More app "registers" here.
};
#pragma pack(pop)

extern app_regs_t app_regs;

/**
 * \brief declare pins inputs or outputs.
 * \warning this status can be overwritten if a pin is later assigned to a PWMTask
 */

void set_port_direction(msg_t& msg);

/**
 * \brief read the 8-channel port simultaneousy
 */
void read_from_port(uint8_t reg_address);

/**
 * \brief write to the 8-channel port as a group. (Values for pins specified
 *  as inputs will be ignored.)
 */
void write_to_port(msg_t& msg);

/**
 * \brief Create a PWMTask.
 */
void write_pwm_task(msg_t& msg);

void write_schedule_ctrl(msg_t& msg);

void write_trigger_type(msg_t& msg);

/**
 * \brief update the app state. Called in a loop.
 */
void update_app_state();

/**
 * \brief reset the app.
 */
void reset_app();

#endif // CUTTLEFISH_APP_H
