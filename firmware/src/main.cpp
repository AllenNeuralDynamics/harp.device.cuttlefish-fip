#include <pico/stdlib.h>
#include <cstring>
#include <config.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#include <cuttlefish_app.h>
#include <schedule_ctrl_queues.h>
#include <pico/multicore.h>
#include <hardware/structs/bus_ctrl.h>
#include <core1_main.h>

queue_t pwm_task_setup_queue;
queue_t cmd_signal_queue;
queue_t schedule_error_signal_queue;

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
HarpCApp& app = HarpCApp::init(who_am_i, hw_version_major, hw_version_minor,
                               assembly_version,
                               harp_version_major, harp_version_minor,
                               fw_version_major, fw_version_minor,
                               serial_number, "Cuttlefish",
                               (uint8_t*)GIT_HASH,
                               &app_regs, app_reg_specs,
                               reg_handler_fns, reg_count, update_app_state,
                               reset_app);

// Core0 main.
int main()
{
    // Init Synchronizer.
    HarpSynchronizer::init(SYNC_UART, HARP_SYNC_RX_PIN);
    app.set_synchronizer(&HarpSynchronizer::instance());
    // Configure core1 to have high priority on the bus.
    bus_ctrl_hw->priority = 0x00000010;
    // Initialize queues for multicore communication.
    queue_init(&pwm_task_setup_queue, sizeof(pwm_task_specs_t), 8);
    queue_init(&cmd_signal_queue, sizeof(uint8_t), 2);
    queue_init(&schedule_error_signal_queue, sizeof(uint8_t), 2);

#if defined(DEBUG) || defined(PROFILE_CPU)
#warning "Initializing printf from UART will slow down core1 main loop."
    stdio_uart_init_full(DEBUG_UART, 921600, DEBUG_UART_TX_PIN, -1);
#endif

    // For testing: schedule some waveforms on core1.
    //pwm_task_specs_t pwm0{0, 5, 10, (1u << LED1)|(1u << PORT_BASE)}; // only works if we don't do anything else.
    //pwm_task_specs_t pwm0{0, 20, 40, (1u << LED1)|(1u << PORT_BASE)};
    //pwm_task_specs_t pwm1{0, 50, 100, (1u << LED0)};

    // Stress test.
    //pwm_task_specs_t pwm0{0, 2000, 4000, (1u << LED1)|(1u << PORT_BASE)};
    //pwm_task_specs_t pwm1{0, 5000, 10000, (1u << (PORT_BASE+1))};
    //pwm_task_specs_t pwm2{0, 2200, 4000, (1u << (PORT_BASE+2))};
    //pwm_task_specs_t pwm3{0, 5400, 10000, (1u << PORT_BASE+3)};
    //pwm_task_specs_t pwm4{0, 1300, 4000, (1u << (PORT_BASE+4))};
    //pwm_task_specs_t pwm5{0, 400, 800, (1u << (PORT_BASE+5))};
    //pwm_task_specs_t pwm6{0, 6000, 10000, (1u << (PORT_BASE+6))};
    //pwm_task_specs_t pwm7{0, 250, 500, (1u << LED0)|(1u << PORT_BASE+7)};
    //queue_try_add(&pwm_task_setup_queue, &pwm0);
    //queue_try_add(&pwm_task_setup_queue, &pwm1);
    //queue_try_add(&pwm_task_setup_queue, &pwm2);
    //queue_try_add(&pwm_task_setup_queue, &pwm3);
    //queue_try_add(&pwm_task_setup_queue, &pwm4);
    //queue_try_add(&pwm_task_setup_queue, &pwm5);
    //queue_try_add(&pwm_task_setup_queue, &pwm6);
    //queue_try_add(&pwm_task_setup_queue, &pwm7);

    //// Start the queue.
    //uint8_t start_signal = 1;
    //queue_try_add(&cmd_signal_queue, &start_signal);

    multicore_reset_core1();
    (void)multicore_fifo_pop_blocking(); // Wait until core1 is ready.
    multicore_launch_core1(core1_main);
    reset_app(); // Setup GPIO states. Get scheduler ready.
    // Loop forever.
    while(true)
        app.run();
}
