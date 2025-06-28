#ifndef FIP_CTRL_QUEUES_H
#define FIP_CTRL_QUEUES_H
#include <pico/util/queue.h>
#include <laser_fip_task.h>

// Container to unpack laser task index and settings from a received harp message.
struct ReconfigureTaskData
{
    size_t task_index;
    LaserFIPTaskSettings settings;
};

// Container to unpack rising edge event info from a received harp message.
struct RisingEdgeEventData
{
    uint32_t output_state;
    uint64_t time_us;
};

// Queues for multicore communication.
extern queue_t set_tasks_state_queue;
extern queue_t add_task_queue;
extern queue_t remove_task_queue;
extern queue_t clear_tasks_queue;
extern queue_t reconfigure_task_queue;
extern queue_t rising_edge_event_queue;

#endif // SCHEDULE_CTRL_QUEUES_H
