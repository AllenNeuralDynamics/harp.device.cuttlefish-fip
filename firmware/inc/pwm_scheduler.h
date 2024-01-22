#ifndef PWM_SCHEDULER_H
#define PWM_SCHEDULER_H
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/irq.h>
#include <pwm_task.h>
#include <etl/priority_queue.h>
#ifdef DEBUG
    #include <cstdio> // for printf
#endif


#define NUM_ENTRIES (64)
#define NUM_TTL_IOS (8)


class PWMScheduler
{
public:
    PWMScheduler();
    ~PWMScheduler();

    bool schedule_pwm_task(PWMTask&& pwm_task)
    {
        if (pq_.size() == pq_.max_size())
            return false;
        pq_.push(pwm_task);
        printf("Pushed task\r\n");
        printf("Task: (%d, %d, %d, %d)\r\n",
                pwm_task.delay_us_, pwm_task.on_time_us_, pwm_task.period_us_, pwm_task.pin_);
        return true;
    }
    void start();
    void clear();

    friend int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

/**
 * \brief called periodically. Sets up next PWMTask to occur on a timer.
 */
    void update();

/**
 * \brief absolute time before which the priority queue needs to be updated.
 */
    uint64_t next_update_time_us_;

private:
    etl::vector<std::reference_wrapper<PWMTask>, NUM_TTL_IOS> next_tasks_; /// container for next simultaneous events.
    etl::priority_queue<PWMTask,
                        NUM_ENTRIES,
                        etl::vector<PWMTask, NUM_ENTRIES>,
                        etl::greater<>> pq_; // TODO: Do we need to pass PWMTask into etl::greater?

    volatile uint32_t next_gpio_port_state_;
    volatile uint32_t next_gpio_port_mask_;
    volatile bool alarm_queued_;
};
#endif // PWM_SCHEDULER_H
