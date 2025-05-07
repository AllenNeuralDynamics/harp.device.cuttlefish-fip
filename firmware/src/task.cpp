#include <task.h>

void Task::update()
{
    // Apply the next pulse event
    update_outputs();
    // Calculate the next update
    ++event_index_;
    if (event_index_ == event_count_)
    {
        event_index_ = 0;
        ++loops_;
    }
    // Recompute next update time.
    if (event_index_ == 0)
        next_update_time_us_ += period_us_ - events_[event_count_ - 1]->us
                                + events_[0]->us;
    else
        next_update_time_us_ += events_[event_index_]->us - events_[event_index_-1]->us;
#if (DEBUG)
    printf("update completed.\r\n");
#endif
}

void Task::start()
{
#if (DEBUG)
    printf("starting task.\r\n");
#endif
    event_index_ = 0;
    loops_ = 0;
    start_time_us_ = timer_hw->timerawl;
    next_update_time_us_ = start_time_us_;
    // TODO: should we instead compute the next time to update as relative
    // to an absolute start time?
    update();
}


void Task::reset()
{
    event_index_ = 0;
    loops_ = 0;
}
