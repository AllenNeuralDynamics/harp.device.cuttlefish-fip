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
        next_update_time_us_ += events_[event_index_]->us;
    printf("update completed.\r\n");
}

void Task::start()
{
    printf("starting task.\r\n");
    event_index_ = 0;
    loops_ = 0;
    start_time_us_ = timer_hw->timerawl;
    next_update_time_us_ = start_time_us_;
    // TODO: should we instead compute the next time to update as relative
    // to an absolute start time?
    update();
}

