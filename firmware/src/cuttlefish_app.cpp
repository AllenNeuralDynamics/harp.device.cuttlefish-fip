#include <cuttlefish_app.h>


PWMScheduler __not_in_flash("pwm_schedule") pwm_schedule;

// Allocate space for up to 8 tasks. Put data structure in RAM so as to avoid
// regular flash access.
etl::vector<PWMTask, 8> __not_in_flash("pwm_tasks") pwm_tasks;

app_regs_t app_regs;

// Define "specs" per-register
RegSpecs app_reg_specs[reg_count]
{
    {(uint8_t*)&app_regs.port_dir, sizeof(app_regs.port_dir), U8},
    {(uint8_t*)&app_regs.port_raw, sizeof(app_regs.port_raw), U8},
    {(uint8_t*)&app_regs.pwm_task, sizeof(app_regs.pwm_task), U8},
    {(uint8_t*)&app_regs.trigger_type, sizeof(app_regs.trigger_type), U8},
    {(uint8_t*)&app_regs.pin_trigger_mask, sizeof(app_regs.pin_trigger_mask), U8},
    {(uint8_t*)&app_regs.pin_trigger_state, sizeof(app_regs.pin_trigger_state), U8},
    {(uint8_t*)&app_regs.time_trigger_us, sizeof(app_regs.time_trigger_us), U32},
    {(uint8_t*)&app_regs.schedule_ctrl, sizeof(app_regs.schedule_ctrl), U8},
    // More specs here if we add additional registers.
};

RegFnPair reg_handler_fns[reg_count]
{
    {HarpCore::read_reg_generic, set_port_direction},
    {read_from_port, write_to_port},
    {HarpCore::read_reg_generic, write_pwm_task},
    {HarpCore::read_reg_generic, write_trigger_type},
    {HarpCore::read_reg_generic, HarpCore::write_reg_generic},
    {HarpCore::read_reg_generic, HarpCore::write_reg_generic},
    {HarpCore::read_reg_generic, HarpCore::write_reg_generic},
    {HarpCore::read_reg_generic, write_schedule_ctrl},
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
    HarpCore::copy_msg_payload_to_register(msg);
    // Interpret byte array as packed function arguments to create a PWMTask.
    pwm_task_specs_t& specs = *((pwm_task_specs_t*)(&msg.payload));
    // Create the task.
    pwm_tasks.push_back(PWMTask(specs.offset_us,
                                specs.on_time_us,
                                specs.period_us,
                                ((uint32_t)specs.port_mask << PORT_BASE)));
    // Schedule it.
    pwm_schedule.schedule_pwm_task(pwm_tasks.back());
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

void write_trigger_type(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // FIXME: implement this.
    // If software trigger,
    //pwm_schedule.start();
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void update_app_state()
{
    // internally only updates as-needed.
    pwm_schedule.update();
}

void reset_app()
{
    // init all pins used as GPIOs.
    gpio_init_mask((0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE));
    // Reset PORT to all inputs. Reset DIR to all outputs.
    //gpio_set_dir_masked((0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE),
    //                    (0xFF << PORT_DIR_BASE)); // 1 is output; 0 is input.
    // Use 8 DIR pins to dictate the overall IO direction of the TTL buffer chips.
    // i.e: all of our control pins will be outputs.
    gpio_set_dir_masked((0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE),
                        (0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE));
    // Default behavior: set each chip as an output and drive a 0.
    gpio_put_masked((0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE),
                    (0xFF << PORT_DIR_BASE));
}
