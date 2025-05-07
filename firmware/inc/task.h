#ifndef TASK_H
#define TASK_H
#include <cstdint>
#include <cstdio>
#include <hardware/timer.h>


/**
 * \brief generic event.
 */
struct event_t
{
    uint32_t us;        /// The time at which the pin_state takes place.
    void(*func_ptr)();  /// any callback fn for implementing observer pattern.

    // Set a default constructor with unused func ptr.
    event_t(uint32_t us, void(*func_ptr)() = nullptr)
    : us(us), func_ptr(func_ptr){}
};


/**
 * \brief Task abstract base class.
 * \details A Task is a one-shot or repeating sequence of zero or more events.
 *  Serived classes have various options for implementing custom behavior.
 *  They can:
 *  - create custom event types by deriving from events, implementing
 *    update_outputs(), and then relying on the builtin update() method to
 *    manage them.
 *  - ignore events altogether and just implement a custom update() method.
 */
class Task
{
public:

    friend class Scheduler;

/**
 * \brief constructor.
 * \param events reference to array of pointers to event objects.
 * \param event_count number of events.
 */
    Task(event_t** events, size_t event_count,
         uint32_t period_us, size_t count = 0)
    :events_(events), event_count_(event_count), period_us_(period_us),
     count_(count),
     event_index_(0), loops_(0){};

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
    virtual void update();

    virtual void update_outputs() = 0;

/**
 * \brief comparison operator for scheduling.
 */
    friend bool operator<(const Task& lhs, const Task& rhs)
    {return int32_t(rhs.next_update_time_us_ - lhs.next_update_time_us_) > 0;}

    virtual void reset();

    virtual void start();

    //inline void start_at_time(uint32_t future_start_time_us)
    //{
    //    next_update_time_us_ = future_start_time_us;
    //}

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
    virtual inline bool requires_future_update()
    {return ((count_ == 0) || (loops_ < count_));}


/**
 * \brief read-only public wrapper for the next absolute time that this
 *  instance must update.
 */
    const virtual inline uint32_t next_update_time_us()
    {return next_update_time_us_;}

protected:
/**
 * \brief absolute time that the state machine needs to update.
 */
    uint32_t next_update_time_us_;

    event_t** events_; // pointer to array of event_t pointers.
    size_t event_count_;

    const size_t count_; /// How many task iterations to execute.
    const uint32_t period_us_; /// Length of one task cycle. Should be at least
                               /// as long as the last event.
    size_t loops_; /// How many task iterations we have executed.

    size_t event_index_;

    uint32_t start_time_us_; /// What (32-bit) time the pulse started.
};
#endif // TASK_H
