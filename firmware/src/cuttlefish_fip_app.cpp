#include <cuttlefish_fip_app.h>

app_regs_t app_regs;

RegSpecs app_reg_specs[REG_COUNT]
{
    {(uint8_t*)*&app_regs.EnableTaskSchedule, sizeof(app_regs.EnableTaskSchedule), U8},
    {(uint8_t*)*&app_regs.AddLaserTask, sizeof(app_regs.AddLaserTask), U8},
    {(uint8_t*)*&app_regs.RemoveLaserTask, sizeof(app_regs.RemoveLaserTask), U8},
    {(uint8_t*)*&app_regs.RemoveAllLaserTasks, sizeof(app_regs.RemoveAllLaserTasks), U8},
    {(uint8_t*)*&app_regs.LaserTaskCount, sizeof(app_regs.LaserTaskCount), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask0, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask1, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask2, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask3, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask4, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask5, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask6, sizeof(LaserFIPTaskSettings), U8},
    {(uint8_t*)*&app_regs.ReconfigureLaserTask7, sizeof(LaserFIPTaskSettings), U8},
};

RegFnPair reg_handler_fns[REG_COUNT]
{
    using namespace HarpCore;
    {read_reg_generic, write_enable_task_schedule}, // read is undefined
    {read_reg_generic, write_add_laser_task},       // read is undefined
    {read_reg_generic, write_remove_laser_task},    // read is undefined
    {read_reg_generic, write_laser_task_count},

    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
    {read_reconfigure_laser_task, write_reconfigure_laser_task},
};

void read_add_laser_task(uint8_t address)
{
    // Undefined. Value returned is meaningless.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(READ, address);
}
void read_remove_laser_task(uint8_t address); // stub?
void read_remove_all_laser_takss(uint8_t address); // stub?
void read_laser_task_count(uint8_t address);
void read_reconfigure_laser_task(uint8_t address);

void write_enable_task_schedule(msg_t& msg)
{
    // TODO: implement this.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_add_laser_task(msg_t& msg);
void write_remove_laser_task(msg_t& msg);
void write_remove_all_laser_taks(msg_t& msg);
void write_laser_task_count(msg_t& msg);
void write_reconfigure_laser_task(msg_t& msg);

void update_app()
{

}

void reset_app()
{

}
