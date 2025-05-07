#include <cuttlefish_fip_app.h>

app_regs_t app_regs;

RegSpecs app_reg_specs[REG_COUNT]
{
    {(uint8_t*)&app_regs.EnableTaskSchedule, sizeof(app_regs.EnableTaskSchedule), U8},
    {(uint8_t*)&app_regs.AddLaserTask, sizeof(app_regs.AddLaserTask), U8},
    {(uint8_t*)&app_regs.RemoveLaserTask, sizeof(app_regs.RemoveLaserTask), U8},
    {(uint8_t*)&app_regs.RemoveAllLaserTasks, sizeof(app_regs.RemoveAllLaserTasks), U8},
    {(uint8_t*)&app_regs.LaserTaskCount, sizeof(app_regs.LaserTaskCount), U8},
    {(uint8_t*)&app_regs.RisingEdgeEvent, sizeof(app_regs.RisingEdgeEvent), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[0], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[1], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[2], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[3], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[4], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[5], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[6], sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)&app_regs.ReconfigureLaserTask[7], sizeof(LaserFIPTaskSettings), U8},
};

RegFnPair reg_handler_fns[REG_COUNT]
{
    {HarpCore::read_reg_generic, write_enable_task_schedule}, // read is technically undefined
    {HarpCore::read_reg_generic, write_add_laser_task},       // read is technically undefined
    {HarpCore::read_reg_generic, write_remove_laser_task},    // read is technically undefined
    {HarpCore::read_reg_generic, write_remove_all_laser_tasks}, // read is technically undefined
    {HarpCore::read_reg_generic, HarpCore::write_to_read_only_reg_error},
    {HarpCore::read_reg_generic, HarpCore::write_to_read_only_reg_error},

    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
};

void read_reconfigure_laser_task(uint8_t address)
{
    size_t task_index = get_fip_task_index(address);
    // Warning: if we are trying to read from a non-configured task, the
    // data is undefined (will be all zeros in this case.).
    if ((task_index + 1) <= app_regs.LaserTaskCount)
        app_regs.ReconfigureLaserTask[task_index] = fip_tasks[task_index].settings_;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(READ, address);
}

bool set_task_schedule_state(bool state)
{
    // Push enable/disable signal to core1.
    bool success = queue_try_add(&enable_task_schedule_queue, &state);
    // Harp register should represent the actual state of the task schedule.
    app_regs.EnableTaskSchedule = uint8_t(state);
    return success;
}

void write_enable_task_schedule(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    bool success = set_task_schedule_state(bool(app_regs.EnableTaskSchedule));
    if (HarpCore::is_muted())
        return;
    if (success)
        HarpCore::send_harp_reply(WRITE, msg.header.address);
    else
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
}

void write_add_laser_task(msg_t& msg)
{
    // Emit error if schedule is running or task count is at maximum.
    if (app_regs.EnableTaskSchedule || (app_regs.LaserTaskCount == MAX_TASK_COUNT))
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    HarpCore::copy_msg_payload_to_register(msg);
    LaserFIPTaskSettings* settings_ptr
        = reinterpret_cast<LaserFIPTaskSettings*>(msg.payload);
    // Emit error if pwm_pin_bit is specified wrong (more than 1 or none).
    if (std::popcount(settings_ptr->pwm_pin_bit) != 1)
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    // Source is one-hot encoded and refers to pins in a range from 0 through 7.
    // PCB "IO0" = GPIO0 + PORT_BASE. Do offset.
    settings_ptr->pwm_pin_bit = settings_ptr->pwm_pin_bit << PORT_BASE;
    settings_ptr->output_mask = settings_ptr->output_mask << PORT_BASE;

    // Push the task settings to core1.
    if (!queue_try_add(&add_task_queue, settings_ptr))
    {
        // Handle queue full error.
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }

    ++app_regs.LaserTaskCount;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_remove_laser_task(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    size_t task_index = app_regs.RemoveLaserTask;

    // Emit error if schedule is running or task does not exist.
    if (app_regs.EnableTaskSchedule || 
        ((task_index + 1 ) > app_regs.LaserTaskCount || (task_index >= MAX_TASK_COUNT)))
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }

    // push remove-by-index signal to core1.
    if (!queue_try_add(&remove_task_queue, &task_index))
    {
        // Handle queue full error.
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }

    // Remove the task from the app_regs and shift all tasks down by one.
    for (size_t i = task_index; i < (MAX_TASK_COUNT - 1); ++i)
    {
        app_regs.ReconfigureLaserTask[i] = app_regs.ReconfigureLaserTask[i + 1];
    }

    --app_regs.LaserTaskCount;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_remove_all_laser_tasks(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // push remove-all signal to core1.
    if (!queue_try_add(&clear_tasks_queue, &app_regs.RemoveAllLaserTasks))
    {
        // Handle queue full error.
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }

    // Clear the app_regs.
    for (size_t i = 0; i < MAX_TASK_COUNT; ++i)
    {
        app_regs.ReconfigureLaserTask[i] = LaserFIPTaskSettings();
    }

    app_regs.LaserTaskCount = 0;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_reconfigure_laser_task(msg_t& msg)
{
    size_t task_index = get_fip_task_index(msg);
    // Emit error if schedule is running.
    if (app_regs.EnableTaskSchedule)
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    // Emit Write Error if this task does not yet exist in the queue.
    if ((task_index + 1) > app_regs.LaserTaskCount)
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    HarpCore::copy_msg_payload_to_register(msg);
    LaserFIPTaskSettings* settings_ptr
        = reinterpret_cast<LaserFIPTaskSettings*>(msg.payload);
    // Emit error if pwm_pin_bit is specified wrong (more than 1 or none).
    if (std::popcount(settings_ptr->pwm_pin_bit) != 1)
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    // Source is one-hot encoded and refers to pins in a range from 0 through 7.
    // PCB "IO0" = GPIO0 + PORT_BASE. Do offset.
    settings_ptr->pwm_pin_bit = settings_ptr->pwm_pin_bit << PORT_BASE;
    settings_ptr->output_mask = settings_ptr->output_mask << PORT_BASE;

    ReconfigureTaskData task_data = {task_index, *settings_ptr};

    // Push reconfigured task data to core1.
    if (!queue_try_add(&reconfigure_task_queue, &task_data))
    {
        // Handle queue full error.
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }

    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void update_app()
{
    // Receive msgs from core1 with state/timings.
    if (!queue_is_empty(&rising_edge_event_queue))
    {
        // Retrieve the rising edge event data from the queue.
        RisingEdgeEventData event_data;
        queue_remove_blocking(&rising_edge_event_queue, &event_data);
        // Offset to account for the GPIO to IO mapping.
        app_regs.RisingEdgeEvent = uint8_t(event_data.output_state >> PORT_BASE);
        //  Send them back over Harp Protocol with a Harp clock domain timestamp.
        HarpCore::send_harp_reply(EVENT, AppRegNum::RisingEdgeEvent,
                                  HarpCore::system_to_harp_us_64(event_data.time_us));
    }
    // Disable output waveforms if we've disconnected com ports (safety feature).
    if (HarpCore::get_op_mode() != ACTIVE)
        set_task_schedule_state(false);
}

void reset_app()
{
    // Clear all settings configurations to all zero.
    app_regs.LaserTaskCount = 0;
    // Configure bus switches for software control of the BNC connectors.
    // Init bus switch pins.
    gpio_init_mask((0x000000FF << PORT_DIR_BASE));
    // Set bus switch to all-outputs and drive an output setting for main IO pins.
    gpio_set_dir_masked(0x000000FF << PORT_DIR_BASE, 0xFFFFFFFF);
    gpio_put_masked(0x000000FF << PORT_DIR_BASE, 0xFFFFFFFF);

    //TODO:  reset core1?.
}
