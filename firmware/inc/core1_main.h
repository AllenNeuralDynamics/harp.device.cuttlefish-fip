#ifndef CORE1_MAIN_H
#define CORE1_MAIN_H
#include <pico/stdlib.h>
#include <config.h>
#include <fip_schedule.h>
#if defined(DEBUG)
    #include <cstdio> // for printf
#endif

void core1_main();

#endif // CORE1_MAIN_H
