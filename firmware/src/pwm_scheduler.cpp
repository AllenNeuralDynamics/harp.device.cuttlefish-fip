#include <pwm_scheduler.h>
// Declare friend function prototype.
int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data);

PWMScheduler::PWMScheduler()
:alarm_queued_{false}
{}

PWMScheduler::~PWMScheduler()
{}

void PWMScheduler::start()
{
    uint64_t now = time_us_64(); // TODO: should be Harp time?
    printf("starting schedule at: %lld.\r\n", now);
    // Iterating through priority queue elements is clunky.
    for (uint8_t i = 0; i < pq_.size(); ++i)
    {
        PWMTask& task = pq_.top().get();
        pq_.pop();
        printf("Task %d old start time: %lld || ", task.pin(), task.start_time_us_);
        task.start_at_time(now);
        printf("new start time: %lld\r\n", task.start_time_us_);
        pq_.push(task); // reschedule at the new time.
    }
    PWMTask& task0 = pq_.top().get();
    pq_.pop();
    PWMTask& task1 = pq_.top().get();
    pq_.pop();
    printf("Popped two tasks: Pin(%d) and Pin(%d)\r\n.", task0.pin(), task1.pin());
    pq_.push(task0);
    pq_.push(task1);
}

void PWMScheduler::clear()
{}


void PWMScheduler::update()
{
    // Prevent queuing another alarm until the port state change has occured.
    if (alarm_queued_)
        return;
    next_gpio_port_mask_ = 0;
    next_gpio_port_state_ = 0;
    uint64_t next_pwm_task_update_time_us = pq_.top().get().next_update_time_us();
    //printf("next update time is: %lld\r\n", next_pwm_task_update_time_us);
    //printf("next update time for pin %d is %lld.\r\n", pwm->pin_, next_pwm_task_update_time_us);
    for (uint8_t i = 0; i < NUM_TTL_IOS; ++i)
    {
        // Pop the highest priority (must update soonest) PWM task.
        PWMTask& pwm = pq_.top().get();
        pq_.pop();
        //printf("popped pin task: %d\r\n", pwm.pin());
        // Update this PWM state and the next time that it needs to be called.
        // Skip gpio action since we will fire all pins of all PWMTasks at once.
        pwm.update(true, true); // force = true; skip_output_action = true.
        // Update the queued gpio port state.
        next_gpio_port_mask_ |= (1u << pwm.pin());
        next_gpio_port_state_ &= ~(1u << pwm.pin());
        next_gpio_port_state_ |= (pwm.state() << pwm.pin());
        // Debug:
        //printf("Next pin task (%d) update time is: %lld\r\n",
        //       pq_.top().get().pin(), pq_.top().get().next_update_time_us());
        // Put this task back in the pq.
        pq_.push(pwm);
        //printf("Pushed pin %d. top element is now pin %d\r\n", pwm.pin(), pq_.top().get().pin());
        // Continue scheduling all PWM tasks that will fire simultaneously.
        if (pq_.top().get().next_update_time_us() != next_pwm_task_update_time_us)
            break;
    }
    //printf("queueing alarm at %lld  || ", next_pwm_task_update_time_us);
    //printf("mask: 0x%x, state: 0x%x\r\n", next_gpio_port_mask_, next_gpio_port_state_);
    //printf("\r\n");
    // Track (1) that we're queueing an alarm and (2) the next time the
    // scheduler neeeds to be updated. Do this first in case the alarm fires
    // immediately.
    alarm_queued_ = true;
    next_update_time_us_ = pq_.top().get().next_update_time_us();
    // Schedule the GPIO port state change!
    // Schedule alarm with ptr to class instance as a parameter.
    // TODO: figure out how to disambiguate Harp/Pico time.
    add_alarm_at(next_pwm_task_update_time_us, set_new_ttl_pin_state, this, true);
}

int64_t set_new_ttl_pin_state(alarm_id_t id, void* user_data)
{
    PWMScheduler& self = *((PWMScheduler*)user_data);
    gpio_put_masked(self.next_gpio_port_mask_, self.next_gpio_port_state_);
    self.alarm_queued_ = false;
    //printf("fired.\r\n");
    return 0; // Do not reschedule.
}
