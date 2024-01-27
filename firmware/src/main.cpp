#include <pico/stdlib.h>
#include <cstring>
#include <config.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#ifdef DEBUG
    #include <stdio.h>
    #include <cstdio> // for printf
#endif
#include <cuttlefish_app.h>

#include <hardware/timer.h>

// Cannot be called inside an interrupt.
inline uint64_t time_us_64_unsafe()
{
    //uint32_t status = save_and_disable_interrupts();
    uint64_t time = timer_hw->timelr; // Locks time until we read TIMEHR.
    //restore_interrupts(status);
    return (uint64_t(timer_hw->timehr) << 32) | time;
}

#ifdef PROFILE_CPU
#define SYST_CSR (*(volatile uint32_t*)(PPB_BASE + 0xe010))
#define SYST_CVR (*(volatile uint32_t*)(PPB_BASE + 0xe018))

#define PRINT_LOOP_INTERVAL_US (16666) // ~60Hz
#endif

// Create device name array.
const uint16_t who_am_i = CUTTLEFISH_HARP_DEVICE_ID;
const uint8_t hw_version_major = 0;
const uint8_t hw_version_minor = 0;
const uint8_t assembly_version = 0;
const uint8_t harp_version_major = 0;
const uint8_t harp_version_minor = 0;
const uint8_t fw_version_major = 0;
const uint8_t fw_version_minor = 0;
const uint16_t serial_number = 0;

// Create Core.
//HarpCApp& app = HarpCApp::init(who_am_i, hw_version_major, hw_version_minor,
//                               assembly_version,
//                               harp_version_major, harp_version_minor,
//                               fw_version_major, fw_version_minor,
//                               serial_number, "Cuttlefish",
//                               &app_regs, app_reg_specs,
//                               reg_handler_fns, reg_count, update_app_state,
//                               reset_app);

// Core0 main.
int main()
{
#ifdef DEBUG
    stdio_uart_init_full(uart0, 921600, UART_TX_PIN, -1); // use uart1 tx only.
    //stdio_usb_init();
    //while (!stdio_usb_connected()){} // Block until connection to serial port.
    printf("Hello, from an RP2040!\r\n");
#endif
#ifdef PROFILE_CPU
    // Configure SYSTICK register to tick with CPU clock (125MHz) and enable it.
    SYST_CSR |= (1 << 2) | (1 << 0);
    uint32_t loop_start_cpu_cycle;
    uint32_t prev_print_time_us;
    uint32_t curr_time_us;
    uint32_t cpu_cycles;
#endif

    // Init Synchronizer.
    //HarpSynchronizer::init(uart0, HARP_SYNC_RX_PIN);
    //app.set_synchronizer(&HarpSynchronizer::instance());
    //reset_app();

    // Schedule some waveforms.
    pwm_tasks.push_back(PWMTask(0, 50000, 100000, (1u << 25)));
    pwm_tasks.push_back(PWMTask(0, 25000, 50000, (1u << 24)));
    //pwm_tasks.push_back(PWMTask(0, 500, 1000, 23));
    //pwm_tasks.push_back(PWMTask(0, 500, 1000, 22));
    //pwm_tasks.push_back(PWMTask(0, 500, 1000, 21));
    //pwm_tasks.push_back(PWMTask(0, 500, 1000, 20));
    //pwm_tasks.push_back(PWMTask(0, 500, 1000, 19));
    //pwm_tasks.push_back(PWMTask(0, 500, 1000, 18));

    pwm_schedule.schedule_pwm_task(pwm_tasks[0]);
    pwm_schedule.schedule_pwm_task(pwm_tasks[1]);
    //pwm_schedule.schedule_pwm_task(pwm_tasks[2]);
    //pwm_schedule.schedule_pwm_task(pwm_tasks[3]);
    //pwm_schedule.schedule_pwm_task(pwm_tasks[4]);
    //pwm_schedule.schedule_pwm_task(pwm_tasks[5]);
    //pwm_schedule.schedule_pwm_task(pwm_tasks[6]);
    //pwm_schedule.schedule_pwm_task(pwm_tasks[7]);

    pwm_schedule.start();

    while(true)
    {
#ifdef PROFILE_CPU
        loop_start_cpu_cycle = SYST_CVR;
        curr_time_us = time_us_64_unsafe();
#endif
        //app.run();
        pwm_schedule.update();
#ifdef PROFILE_CPU
        // SYSTICK is 24bit and counts down.
        cpu_cycles = ((loop_start_cpu_cycle << 8) - (SYST_CVR << 8)) >> 8;
        if ((curr_time_us - prev_print_time_us) < PRINT_LOOP_INTERVAL_US)
            continue;
        if (cpu_cycles <= 60)
            continue;
        prev_print_time_us = curr_time_us;
        printf("cpu cycles/loop: %lu\r\n", cpu_cycles);
#endif
    }
}
