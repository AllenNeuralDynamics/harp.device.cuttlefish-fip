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

HarpCApp& app = HarpCApp::init(WHO_AM_I, HW_VERSION_MAJOR, HW_VERSION_MINOR,
                               ASSEMBLY_VERSION,
                               HARP_VERSION_MAJOR, HARP_VERSION_MINOR,
                               FW_VERSION_MAJOR, FW_VERSION_MINOR,
                               SERIAL_NUMBER, "Cuttlefish",
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
    // Configure core1 to have high bus priority.
    bus_ctrl_hw->priority = 0x00000010;
    // Initialize queues for multicore communication.
    queue_init(&pwm_task_setup_queue, sizeof(pwm_task_specs_t), 8);
    queue_init(&cmd_signal_queue, sizeof(uint8_t), 2);
    queue_init(&schedule_error_signal_queue, sizeof(uint8_t), 2);

#if defined(DEBUG) || defined(PROFILE_CPU)
#warning "Initializing printf from UART will slow down core1 main loop."
    stdio_uart_init_full(DEBUG_UART, 921600, DEBUG_UART_TX_PIN, -1);
#endif

    multicore_reset_core1();
    (void)multicore_fifo_pop_blocking(); // Wait until core1 is ready.
    multicore_launch_core1(core1_main);
    reset_app(); // Setup GPIO states. Get scheduler ready.
    // Loop forever.
    while(true)
        app.run();
}
