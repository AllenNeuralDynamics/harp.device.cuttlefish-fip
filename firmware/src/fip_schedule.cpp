#include <harp_core.h>
#include <fip_schedule.h>
#include <fip_ctrl_queues.h>
#include <hardware/structs/timer.h>

etl::vector<LaserFIPTask, MAX_TASK_COUNT> fip_tasks;

bool enabled = false;

/// \warning: this fn should not be called inside an interrupt.
inline uint64_t time_us_64_unsafe()
{
    uint64_t time = timer_hw->timelr; // Locks time until we read TIMEHR.
    return (uint64_t(timer_hw->timehr) << 32) | time;
}

inline uint32_t time_us_32_fast()
{return timer_hw->timerawl;}

void sleep_us(uint32_t us)
{
    uint32_t start_time_us = timer_hw->timerawl;
    while (int32_t(timer_hw->timerawl - start_time_us) < us)
        tight_loop_contents();
}

void update_enabled_state()
{
    // Update enabled state.
    if (!queue_is_empty(&enable_task_schedule_queue))
    {
        uint8_t enable_state;

        if (queue_try_remove(&enable_task_schedule_queue, &enable_state))
        {
            // Update the enabled state based on the message.
            if (enable_state)
                enabled = true;
            else
                enabled = false;
        }
    }
}

void update_fip_tasks()
{
    LaserFIPTaskSettings task_settings;

    // Check if there are messages in the add task queue.
    while (!queue_is_empty(&add_task_queue))
    {
        // Retrieve the task settings from the queue.
        if (queue_try_remove(&add_task_queue, &task_settings) && fip_tasks.size() < MAX_TASK_COUNT)
        {
            // Add the task to the fip_tasks vector.
            fip_tasks.emplace_back(
                task_settings.pwm_pin,
                task_settings.pwm_duty_cycle,
                task_settings.pwm_frequency_hz,
                task_settings.output_mask,
                task_settings.events,
                task_settings.mute,
                task_settings.delta1_us,
                task_settings.delta2_us,
                task_settings.delta3_us,
                task_settings.delta4_us);
        }
    }

    // Check if there are messages in the remove task queue.
    uint8_t task_index;
    while (!queue_is_empty(&remove_task_queue))
    {
        // Retrieve the task index from the queue.
        if (queue_try_remove(&remove_task_queue, &task_index))
        {
            // Remove the task from the fip_tasks vector.
            if (task_index < fip_tasks.size())
            {
                fip_tasks.erase(fip_tasks.begin() + task_index);
            }
        }
    }

    // Check if there are messages in the clear tasks queue.
    bool clear_all;
    if (!queue_is_empty(&clear_tasks_queue))
    {
        // Retrieve the clear signal from the queue.
        if (queue_try_remove(&clear_tasks_queue, &clear_all))
        {
            if (clear_all)
            {
                // Clear all tasks from the fip_tasks vector.
                fip_tasks.clear();
            }
        }
    }

    // Check if there are messages in the reconfigure task queue.
    ReconfigureTaskData task_data{};
    while (!queue_is_empty(&reconfigure_task_queue))
    {
        // Retrieve the task settings from the queue.
        if (queue_try_remove(&reconfigure_task_queue, &task_data))
        {
            size_t task_index = task_data.task_index;
            LaserFIPTaskSettings task_settings = task_data.settings;

            if (task_index < fip_tasks.size())
            {
                // Reconfigure the task in the fip_tasks vector.
                fip_tasks[task_index] = LaserFIPTask(
                    task_settings.pwm_pin,
                    task_settings.pwm_duty_cycle,
                    task_settings.pwm_frequency_hz,
                    task_settings.output_mask,
                    task_settings.events,
                    task_settings.mute,
                    task_settings.delta1_us,
                    task_settings.delta2_us,
                    task_settings.delta3_us,
                    task_settings.delta4_us);
            }
        }
    }
}

void run()
{
    enabled = false;
    // do a continuous sequence.
    while (true)
    {
        // Check for input from core1.
        update_enabled_state();
        if (!enabled)
            update_fip_tasks();
        if (enabled)
            run_sequence();
    }
}

void push_harp_msg(uint32_t output_state, uint64_t time_us)
{
    // Send rising edge output state to core0.
    RisingEdgeEventData event_data = {output_state, time_us};
    queue_try_add(&rising_edge_event_queue, &event_data);
}

void run_sequence()
{
    for (auto& fip_task: fip_tasks)
        run_exposure(fip_task);
}

inline void run_exposure(LaserFIPTask& fip_task)
{
    // TODO: consider tweaking delays to account for elapsed time to trigger signals.
    //uint32_t start_time_us = time_us_32_fast();
    //uint32_t elapsed_time_us;
    fip_task.laser_.enable_output();
    // Send pin state w/ pwm rising edge.
    push_harp_msg((1u << fip_task.laser_.pin()), time_us_64_unsafe());
    //elapsed_time_us = time_us_32_fast() - start_time_us;
    //sleep_us(fip_task.settings_.delta3_us - elapsed_time_us);
    sleep_us(fip_task.settings_.delta3_us);
    fip_task.set_output();
    // Send pin state w/ CAM_G rising edge.
    push_harp_msg(
        ((1u << fip_task.laser_.pin()) | fip_task.output_mask()),
        time_us_64_unsafe());
    sleep_us(fip_task.settings_.delta1_us);
    fip_task.clear_output();
    sleep_us(fip_task.settings_.delta4_us);
    fip_task.laser_.disable_output();
    sleep_us(fip_task.settings_.delta2_us);
}

