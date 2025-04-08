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

void write_enable_task_schedule(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // TODO: push en/disable signal to core1.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_add_laser_task(msg_t& msg)
{
    // Emit error if schedule is running.
    if (app_regs.EnableTaskSchedule || (app_regs.LaserTaskCount == MAX_TASK_COUNT))
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    HarpCore::copy_msg_payload_to_register(msg);
    LaserFIPTaskSettings* settings_ptr
        = reinterpret_cast<LaserFIPTaskSettings*>(msg.payload);
    // TODO: push added task settings to core1.
    ++app_regs.LaserTaskCount;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_remove_laser_task(msg_t& msg)
{
    size_t task_index = get_fip_task_index(msg);
    // Emit error if schedule is running or task does not exist.
    if (app_regs.EnableTaskSchedule || ((task_index + 1 ) > app_regs.LaserTaskCount))
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    HarpCore::copy_msg_payload_to_register(msg);
    // TODO: push remove-by-index signal to core1.
    --app_regs.LaserTaskCount;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_remove_all_laser_tasks(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // TODO: push remove-all signal to core1.
    app_regs.LaserTaskCount = 0;
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_reconfigure_laser_task(msg_t& msg)
{
    size_t task_index = get_fip_task_index(msg);
    // Emit Write Error if this task does not yet exist in the queue.
    if ((task_index + 1) > app_regs.LaserTaskCount)
    {
        HarpCore::send_harp_reply(WRITE_ERROR, msg.header.address);
        return;
    }
    HarpCore::copy_msg_payload_to_register(msg);
    LaserFIPTaskSettings* settings_ptr
        = reinterpret_cast<LaserFIPTaskSettings*>(msg.payload);
    // TODO: push new task settings to core1 and apply them there.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void update_app()
{
    // TODO: Receive msgs from core1 with state/timings.
    void* new_msg = nullptr; // TODO: implement this.
    //  Send them back over Harp Protocol.
    if (new_msg != nullptr)
        HarpCore::send_harp_reply(EVENT, AppRegNum::RisingEdgeEvent);
}

void reset_app()
{
    // Clear all settings configurations to all zero.
    app_regs.LaserTaskCount = 0;
    // FIXME: probably reset other things too.
    // reset core1.
}
