#ifndef SCHEDULE_CTRL_QUEUES_H
#define SCHEDULE_CTRL_QUEUES_H
#include <pico/util/queue.h>
#ifdef DEBUG
    #include <stdio.h>
    #include <cstdio> // for printf
#endif

// Container to dereference packed binary data for instantiating a PWMTask.
#pragma pack(push, 1)
struct pwm_task_specs_t
{
    uint32_t offset_us;
    uint32_t on_time_us;
    uint32_t period_us;
    uint32_t port_mask;
    uint32_t cycles;
    uint8_t invert;
};
#pragma pack(pop)

extern queue_t pwm_task_setup_queue;
extern queue_t cmd_signal_queue;

#endif // SCHEDULE_CTRL_QUEUES_H
