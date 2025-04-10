#include <pico/stdlib.h>
#include <cstring>
#include <config.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#include <cuttlefish_fip_app.h>
#include <fip_ctrl_queues.h>
#include <pico/multicore.h>
#include <hardware/structs/bus_ctrl.h>
#include <core1_main.h>


HarpCApp& app = HarpCApp::init(0, 0, 0,
                               0,
                               0, 0,
                               0, 0,
                               0, "cuttlefish-fip",
                               (uint8_t*)GIT_HASH,
                               &app_regs, app_reg_specs,
                               reg_handler_fns, REG_COUNT, update_app,
                               reset_app);
/*

HarpCApp& app = HarpCApp::init(WHO_AM_I, HW_VERSION_MAJOR, HW_VERSION_MINOR,
                               ASSEMBLY_VERSION,
                               HARP_VERSION_MAJOR, HARP_VERSION_MINOR,
                               FW_VERSION_MAJOR, FW_VERSION_MINOR,
                               SERIAL_NUMBER, "cuttlefish-fip",
                               (uint8_t*)GIT_HASH,
                               &app_regs, app_reg_specs,
                               reg_handler_fns, REG_COUNT, update_app,
                               reset_app);
*/

// Core0 main.
int main()
{
    // Init Synchronizer.
    HarpSynchronizer::init(SYNC_UART, HARP_SYNC_RX_PIN);
    app.set_synchronizer(&HarpSynchronizer::instance());
    // Configure core1 to have high bus priority.
    bus_ctrl_hw->priority = 0x00000010;
    // Initialize queues for multicore communication.
    queue_init(&enable_task_schedule_queue, sizeof(uint8_t), MAX_QUEUE_SIZE);
    queue_init(&add_task_queue, sizeof(LaserFIPTaskSettings), MAX_QUEUE_SIZE);
    queue_init(&remove_task_queue, sizeof(uint8_t), MAX_QUEUE_SIZE);
    queue_init(&clear_tasks_queue, sizeof(uint8_t), MAX_QUEUE_SIZE);
    queue_init(&reconfigure_task_queue, sizeof(ReconfigureTaskData), MAX_QUEUE_SIZE);
    queue_init(&rising_edge_event_queue, sizeof(RisingEdgeEventData), MAX_QUEUE_SIZE);

#if defined(DEBUG)
#warning "Initializing printf from UART will slow down core1 main loop."
    stdio_uart_init_full(DEBUG_UART, 921600, DEBUG_UART_TX_PIN, -1);
#endif

    // Launch core1 to juggle pwm tasks.
    multicore_reset_core1();
    (void)multicore_fifo_pop_blocking(); // Wait until core1 is ready.
    multicore_launch_core1(core1_main);

    reset_app();
    while(true)
        app.run();
}
