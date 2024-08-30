#ifndef PWM_SCHEDULER_H
#define PWM_SCHEDULER_H
#include <stdint.h>
#include <pico/stdlib.h>
#include <hardware/irq.h>
#include <pwm_task.h>
#include <etl/priority_queue.h>
#include <hardware/timer.h>
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

    void schedule_pwm_task(uint32_t delay_us, uint32_t t_on_us,
                           uint32_t t_period_us, uint32_t pin_mask,
                           uint32_t count, bool invert);
    void schedule_pwm_task(PWMTask task);
/**
 * \brief cancel any active alarms and clear the queue.
 */
    void reset();

/**
 * \brief start the schedule.
 */
    void start();

    inline void clear()
    {reset();}

    friend void set_new_ttl_pin_state(void);
    friend void handle_missed_deadline();


/**
 * \brief called periodically. Sets up next PWMTask to occur on a timer.
 */
    void update(bool first_update = false);

    void cancel_alarm();

/**
 * \brief absolute time before which the priority queue needs to be updated.
 */
    uint64_t next_update_time_us_;

private:
    etl::vector<PWMTask, NUM_TTL_IOS> pwm_tasks_; // Container to hold PWMTasks.
                                                  // We will access them
                                                  // (usually) through the pq_;
/**
 * \brief the priority queue
 * \details We need to use references wrappers to access mutable versions of
 *      queue elements. This is consistent with the std::priority_queue
 *      implementation which returns const references to top().
 */
    etl::priority_queue<std::reference_wrapper<PWMTask>,
                        NUM_ENTRIES,
                        etl::vector<std::reference_wrapper<PWMTask>, NUM_ENTRIES>,
                        etl::greater<std::reference_wrapper<PWMTask>>> pq_;

// FIXME: make this private again.
public:
    static volatile int32_t alarm_num_;
private:
    static volatile uint32_t next_gpio_port_state_;
    static volatile uint32_t next_gpio_port_mask_;
    static volatile bool alarm_queued_;
};
#endif // PWM_SCHEDULER_H
