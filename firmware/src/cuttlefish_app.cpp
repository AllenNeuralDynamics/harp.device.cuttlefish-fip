#include <cuttlefish_app.h>

app_regs_t app_regs;
uint8_t pwm_task_mask; // Record of pins dedicated to running PWMTasks.

// Define "specs" per-register
RegSpecs app_reg_specs[reg_count]
{
    {(uint8_t*)&app_regs.port_dir, sizeof(app_regs.port_dir), U8}, // 32
    {(uint8_t*)&app_regs.port_state, sizeof(app_regs.port_state), U8},  // 33

    // More specs here if we add additional registers.
};

RegFnPair reg_handler_fns[reg_count]
{
    {HarpCore::read_reg_generic, write_port_dir},           // 32
    {read_port_state, write_port_state},                    // 33

    // More handler function pairs here if we add additional registers.
};

void reset_schedule()
{
    // Abort any running task schedule on core1.
    uint8_t abort_schedule = 1u << 1;
    // Signal to core1 to trash all PWMTasks and reset their GPIO pins to input.
    queue_try_add(&cmd_signal_queue, &abort_schedule);
}

void write_port_dir(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // Set Buffer ctrl pins and corresponding IO pin to both match.
    // Omit setting direction of pins used by existing PWM Tasks.
    // Note: additional 0x000000FF masking is because bitwise '~' on a uint8_t
    //  will perform integral promotion automatically for a 32-bit system.
    gpio_put_masked((0x000000FF & uint32_t(~pwm_task_mask)) << PORT_DIR_BASE,
                    uint32_t(app_regs.port_dir) << PORT_DIR_BASE);
    gpio_set_dir_masked((0x000000FF & uint32_t(~pwm_task_mask)) << PORT_BASE,
                        uint32_t(app_regs.port_dir) << PORT_BASE);
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void read_port_state(uint8_t reg_address)
{
    // Include the state of pins driven by PWMTasks.
    app_regs.port_state = uint8_t(gpio_get_all() >> PORT_BASE);
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(READ, reg_address);
}

void write_port_state(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // Exclude pins controlled by PWM tasks.
    uint8_t output_pins = app_regs.port_dir & ~pwm_task_mask;
    // write to output pins (not including pins controlled by PWM Tasks).
    gpio_put_masked(uint32_t(output_pins) << PORT_BASE,
                    uint32_t(app_regs.port_state) << PORT_BASE);
    // Read back what we just wrote since it's fast.
    // Add delay for change to take effect. (May be related to slew rate).
    asm volatile("nop \n nop \n nop");
    app_regs.port_state = uint8_t(gpio_get_all() >> PORT_BASE);
#if defined(DEBUG)
    printf("Wrote to GPIOs. New GPIO state: 0x%08lx\r\n", gpio_get_all());
#endif
    // Reply with the actual value that we wrote.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}



void update_app_state()
{
    // TODO: Dispatch any edge timings or start/stop go signals to core1
    // TODO: Receive any event edge timings from core1 and send them back as events.

    //if (!queue_is_empty(&schedule_error_signal_queue))
    //    queue_try_remove(&schedule_error_signal_queue, &schedule_error);
    //app_regs.error_state |= schedule_error;
    //if (schedule_error != 0)
    //    HarpCore::send_harp_reply(EVENT, 43); // error_state address. FIXME: magic #
}

void reset_app()
{
    // init all pins used as GPIOs.
    gpio_init_mask((0x000000FF << PORT_DIR_BASE) | (0x000000FF << PORT_BASE));
    // TODO: check that we are properly configuring output pins.
    // Reset unbuffered IO pins to inputs.
    gpio_set_dir_masked(0x000000FF << PORT_BASE, 0xFFFFFFFF);
    // Reset Buffer ctrl pins to all outputs and drive an output setting.
    gpio_set_dir_masked(0x000000FF << PORT_DIR_BASE, 0xFFFFFFFF);
    gpio_put_masked(0x000000FF << PORT_DIR_BASE, 1);
    // Reset Harp register struct elements.
    app_regs.port_dir = 0xFF; // all outputs.
    app_regs.port_state = uint8_t(gpio_get_all() >> PORT_BASE);
    reset_schedule();
}
