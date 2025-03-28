#ifndef CORE1_MAIN_H
#define CORE1_MAIN_H
#include <pico/stdlib.h>
#include <config.h>
#include <task.h>
#include <pwm_task.h>
#include <schedule_ctrl_queues.h>
#include <hardware/timer.h>
#if defined(DEBUG) || defined(PROFILE_CPU)
    #include <stdio.h>
    #include <cstdio> // for printf
#endif

uint64_t time_us_64_unsafe();
#endif

void core1_main();

#endif // CORE1_MAIN_H
