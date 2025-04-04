#include <fip_schedule.h>

etl::vector<LaserFIPTask, MAX_TASK_COUNT> fip_tasks;

/// \warning: this fn should not be called inside an interrupt.
inline uint64_t time_us_64_unsafe()
{
    uint64_t time = timer_hw->timelr; // Locks time until we read TIMEHR.
    return (uint64_t(timer_hw->timehr) << 32) | time;
}

void sleep_us(uint32_t us)
{
    uint32_t start_time_us = timer_hw->timerawl;
    while (int32_t(timer_hw->timerawl - start_time_us) < us)
        asm volatile("nop");
}

bool stop_received()
{
    // Do nothing.
    return false;
}

void setup_fip_schedule()
{
    // TODO: refactor to create from settings objects arriving from core0.
    fip_tasks.emplace_back(PWM_470_PIN, 0.5, 10000, (1 << CAM_R_PIN),
                           true, false, DELTA1, DELTA2, DELTA3, DELTA4);
    fip_tasks.emplace_back(PWM_415_PIN, 0.5, 10000, (1 << CAM_R_PIN),
                           true, false, DELTA1, DELTA2, DELTA3, DELTA4);
    fip_tasks.emplace_back(PWM_565_PIN, 0.5, 10000, (1 << CAM_G_PIN),
                           true, false, DELTA1, DELTA2, DELTA3, DELTA4);
}

void run()
{
    // do a continuous sequence.
    while (true)
    {
        run_sequence();
        if (stop_received())
            return;
    }
}

void push_harp_msg(uint32_t output_state, uint64_t time_us)
{
    // TODO: send this to core0.
}

void run_sequence()
{
    // TODO: pick a data structure for adding/removing fip tasks and accessing them by index.
    //  or accessing them by GPIO pin?
    for (auto& fip_task: fip_tasks)
        run_exposure(fip_task);
}

inline void run_exposure(LaserFIPTask& fip_task)
{
    // TODO: tweak delays to account for elapsed time to trigger signals.
    fip_task.laser_.enable_output();
    // Send pin state w/ pwm rising edge.
    push_harp_msg((1u << fip_task.laser_.pin()), time_us_64_unsafe());
    sleep_us(DELTA3);
    fip_task.set_output();
    // Send pin state w/ CAM_G rising edge.
    push_harp_msg(
        ((1u << fip_task.laser_.pin()) | fip_task.output_mask()),
        time_us_64_unsafe());
    sleep_us(CAM_EXPOSURE_TIME);
    fip_task.clear_output();
    sleep_us(DELTA4);
    fip_task.laser_.disable_output();
    sleep_us(DELTA2);
}

