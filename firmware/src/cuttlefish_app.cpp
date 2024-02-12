#include <cuttlefish_app.h>

app_regs_t app_regs;

// Define "specs" per-register
RegSpecs app_reg_specs[reg_count]
{
    {(uint8_t*)&app_regs.port_dir, sizeof(app_regs.port_dir), U8},
    {(uint8_t*)&app_regs.port_raw, sizeof(app_regs.port_raw), U8},
    {(uint8_t*)&app_regs.pwm_task, sizeof(app_regs.pwm_task), U8},
    {(uint8_t*)&app_regs.arm_ext_trigger, sizeof(app_regs.arm_ext_trigger), U8},
    {(uint8_t*)&app_regs.ext_trigger_edge, sizeof(app_regs.ext_trigger_edge), U8},
    {(uint8_t*)&app_regs.sw_trigger, sizeof(app_regs.sw_trigger), U8},
    {(uint8_t*)&app_regs.schedule_ctrl, sizeof(app_regs.schedule_ctrl), U8}
    // More specs here if we add additional registers.
};

RegFnPair reg_handler_fns[reg_count]
{
    {HarpCore::read_reg_generic, set_port_direction},
    {read_from_port, write_to_port},
    {HarpCore::read_reg_generic, write_pwm_task},
    {HarpCore::read_reg_generic, write_arm_ext_trigger},
    {HarpCore::read_reg_generic, write_ext_trigger_edge},
    {HarpCore::read_reg_generic, write_sw_trigger},
    {HarpCore::read_reg_generic, write_schedule_ctrl}
    // More handler function pairs here if we add additional registers.
};

void set_port_direction(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // set gpio buffer direction to outputs (1) or inputs (0).
    gpio_set_dir_masked((0xFF << PORT_DIR_BASE),
                        uint32_t(app_regs.port_dir) << PORT_DIR_BASE);
    // set ttl buffers to outputs or inputs.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void read_from_port(uint8_t reg_address)
{
    app_regs.port_raw = uint8_t(0xFF & (gpio_get_all() >> PORT_BASE));
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(READ, reg_address);
}

void write_to_port(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // write to GPIOs here.
    gpio_put_masked(app_regs.port_dir,
                    (uint32_t(app_regs.port_raw) << PORT_BASE));
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}


void write_pwm_task(msg_t& msg)
{
    //HarpCore::copy_msg_payload_to_register(msg);
    // Interpret byte array as packed function arguments to create a PWMTask.
    pwm_task_specs_t& specs = *((pwm_task_specs_t*)msg.payload);
    // Send the task to core1.
    queue_try_add(&pwm_task_setup_queue, &specs);
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_arm_ext_trigger(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    uint8_t& ext_trigger_mask = *((uint8_t*)msg.payload);
    // FIXME: implement this.
    // Set pin as GPIO.
    // Configure a GPIO external interrupt to start the schedule.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_ext_trigger_edge(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // FIXME: implement this.
    // Check if ext trigger has already been configured. Re-configure if so.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_sw_trigger(msg_t& msg)
{
    const uint8_t& start = *((uint8_t*)msg.payload);
    if (start)
        queue_try_add(&cmd_signal_queue, &start);
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_schedule_ctrl(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // FIXME: implement this.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void update_app_state()
{
}

void reset_app()
{
    // Reset non-zero struct elements.
    app_regs.ext_trigger_edge = 0xFF;

    // init all pins used as GPIOs.
    gpio_init_mask((0x000000FF << PORT_DIR_BASE) | (0x000000FF << PORT_BASE));
    // Reset PORT to all outputs. Reset DIR to all outputs.
    gpio_set_dir_masked((0x000000FF << PORT_DIR_BASE) | (0x000000FF << PORT_BASE),
                        (0x000000FF << PORT_DIR_BASE) | (0x000000FF << PORT_BASE));
    // Default behavior: set each chip as an output and drive a 0.
    gpio_put_masked((0x000000FF << PORT_DIR_BASE) | (0x000000FF << PORT_BASE),
                    (0x000000FF << PORT_DIR_BASE));

    // TODO: clear schedule.
}
