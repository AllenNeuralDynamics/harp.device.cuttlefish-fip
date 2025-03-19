#ifndef TASK_H
#define TASK_H
#include <stdint.h>
#include <hardware/timer.h>

/**
 * \brief Task base class.
 */
class Task
{
public:
    Task() = default;
    ~Task() = default;

/**
 * \brief call in a loop to have the task manage itself.
 * \warning do no call this function if an external entity (a scheduler) is
 *  calling update() manually.
 */
    inline void spin()
    {
        if (time_to_update() && requires_future_update())
            update();
    }

/**
 * \brief should be called when it is time to update.
 */
    virtual void update() = 0;

/**
 * \brief comparison operator for scheduling.
 */
    friend bool operator<(const Task& lhs, const Task& rhs)
    {return int32_t(rhs.next_update_time_us_ - lhs.next_update_time_us_) > 0;}

    virtual void reset() = 0;

    virtual inline void start() = 0;

    inline void start_at_time(uint32_t future_start_time_us)
    {
        next_update_time_us_ = future_start_time_us;
    }

/**
 * \brief stop outputs.
 */
    virtual inline void stop() = 0;



/**
 * \brief true if a task that requires updating due/overdue for an update.
 */
    virtual inline bool time_to_update()
    {return int32_t(timer_hw->timerawl - next_update_time_us_) >= 0;}

/**
 * \brief true if update() must be called again in the future.
 */
    virtual inline bool requires_future_update() = 0;

/**
 * \brief read-only public wrapper for the next absolute time that this
 *  instance must update.
 */
    const virtual inline uint32_t next_update_time_us()
    {return next_update_time_us_;}


private:
    friend class Scheduler;

/**
 * \brief absolute time that the state machine needs to update.
 */
    uint32_t next_update_time_us_;
};
#endif // TASK_H
