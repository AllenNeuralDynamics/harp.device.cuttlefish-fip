#include <core1_main.h>

bool schedule_failed;

__not_in_flash("instances")PWMScheduler pwm_schedule;


#if defined(PROFILE_CPU)
// Note: this fn should not be called inside an interrupt.
inline uint64_t time_us_64_unsafe()
{
    uint64_t time = timer_hw->timelr; // Locks time until we read TIMEHR.
    return (uint64_t(timer_hw->timehr) << 32) | time;
}
#endif

// Override default behavior of this function defined weakly elsewhere.
void handle_missed_deadline()
{
    gpio_init(LED1);
    gpio_set_dir(LED1, 1); // output
    gpio_put(LED1, 1); // turn on auxilary LED.
    schedule_failed = true;
    // TODO: push an error message back to core0.
}

/*
void update_fsm()
{
    // Update inputs.
    // TODO: this.
    // Compute next-state logic.
    uint8_t next_state{state_};
    switch(state_)
    {
        case RUN_SCHEDULE:
            next_state = RUN_SCHEDULE;
            break;
        case LOAD_SCHEDULE:
            next_state = LOAD_SCHEDULE;
            break;
    }
    if (state_ == RUN_SCHEDULE)
    {
    }
    // Handle output logic.
    if (state_ == LOAD_SCHEDULE)
    {

    }

    // Apply outputs.
    state_ = next_state;
}
*/

void run_task_loop()
{
    // Retrieve PWMTask specs from the shared queue.
    uint8_t cmd = 0;
    uint8_t abort_schedule = 0;
    pwm_task_specs_t task_specs;
    // Load and wait for schedule start cmds.
    while(true)
    {
        if(!queue_is_empty(&pwm_task_setup_queue))
        {
            queue_remove_blocking(&pwm_task_setup_queue, &task_specs);
            pwm_schedule.schedule_pwm_task(task_specs.offset_us,
                                           task_specs.on_time_us,
                                           task_specs.period_us,
                                           (uint32_t(task_specs.port_mask) << PORT_BASE),
                                           task_specs.cycles,
                                           bool(task_specs.invert));
        }
        if (!queue_is_empty(&cmd_signal_queue))
            queue_try_remove(&cmd_signal_queue, &cmd);
        if (cmd == 1) // start signal.
            break;
        if (cmd == 1u << 1) // bail early.
        {
            pwm_schedule.reset();
            return;
        }
    }
    // Start after receiving the start signal.
    while(true)
    {
        // Wait for schedule restart cmds.
        while(!cmd) // Skip the first time.
        {
            if (!queue_is_empty(&cmd_signal_queue))
                queue_try_remove(&cmd_signal_queue, &cmd);
        }
        cmd = 0; // Clear received cmd for next iteration.
        gpio_put(LED1, 0); // FIXME: remove this.
        pwm_schedule.start();
        while(true)
        {
            // This while loop needs to be tight so we call update() quickly!
            #if defined(PROFILE_CPU)
            #warning "Profiling this loop causes the first iteration to take longer."
                    loop_start_cpu_cycle = SYST_CVR;
                    curr_time_us = time_us_64_unsafe();
            #endif
            pwm_schedule.update(); // internally only updates as needed.
            // Check for stop signal, scheduler errors, or an idle scheduler.
            if (!queue_is_empty(&cmd_signal_queue))
                queue_try_remove(&cmd_signal_queue, &cmd);
            abort_schedule = cmd & 0x02;
            if (schedule_failed || abort_schedule)
                break;
            #if defined(PROFILE_CPU)
                    // SYSTICK is 24bit and counts down.
                    cpu_cycles = ((loop_start_cpu_cycle << 8) - (SYST_CVR << 8)) >> 8;
                    if (cpu_cycles > max_cpu_cycles)
                        max_cpu_cycles = cpu_cycles;
                    if ((curr_time_us - prev_print_time_us) < PRINT_LOOP_INTERVAL_US)
                        continue;
                    prev_print_time_us = curr_time_us;
                    printf("max cycles/loop: %lu\r\n", max_cpu_cycles);
                    max_cpu_cycles = 0;
            #endif
        }
        if (schedule_failed || abort_schedule)
            break;
    }
    if (schedule_failed)
    {
        // Dispatch error message to core0.
        uint8_t error = 1;
        queue_try_add(&schedule_error_signal_queue, &error);
    }
    if (abort_schedule)
    {

    }
    // FIXME: cleanup GPIOs. Add this to each task in the destructor.
    // Clear existing tasks for next iteration.
    pwm_schedule.reset();
}

// Core1 main.
void core1_main()
{
#if defined(DEBUG)
    printf("Hello from core1.\r\n");
#endif
    // Set DEBUG LED
    gpio_init(LED1);
    gpio_set_dir(LED1, 1); // output
    gpio_put(LED1, 0); // Set off.
    schedule_failed = false; // TODO: consider a struct for this.
#if defined(PROFILE_CPU)
    // Configure SYSTICK register to tick with CPU clock (125MHz) and enable it.
    SYST_CSR |= (1 << 2) | (1 << 0);
    uint32_t loop_start_cpu_cycle;
    uint32_t prev_print_time_us;
    uint32_t curr_time_us;
    uint32_t cpu_cycles;
    uint32_t max_cpu_cycles = 0;
#endif
    while (true)
        run_task_loop();
}
