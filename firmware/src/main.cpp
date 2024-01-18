#include <pico/stdlib.h>
#include <cstring>
#include <config.h>
#include <harp_message.h>
#include <harp_core.h>
#include <harp_c_app.h>
#include <harp_synchronizer.h>
#ifdef DEBUG
    #include <cstdio> // for printf
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

// Setup for Harp App
const size_t reg_count = 8;

#pragma pack(push, 1)
struct app_regs_t
{
    volatile uint8_t port_dir; // 1 = output; 0 = input.
    volatile uint8_t port_raw; // current gpio state. Readable and writeable.
    volatile uint8_t schedule_ctrl; // [0] :  clear schedule
                                    // [1] : dump schedule
    volatile uint8_t trigger_type;  // [0]=1 : absolute time stored in
                                    //         trigger_time_us starts th
                                    //         schedule.
                                    // [1]=1 : gpio pin configured as input
                                    //         starts the schedule.
                                    // [3:2]=xx  : pin rising-edge [00],
                                    //             falling-edge [01], or
                                    //             change[11]
                                    //             starts the schedule.
                                    // [7]=1 : start the schedule now
                                    //         (i.e: software trigger).
                                    // reset value = 0.
    volatile uint8_t pin_trigger_mask; // which port pins (configured as input)
                                       // cause the schedule to trigger.
    volatile uint8_t pin_trigger_state; // value of the gpio pin(s) that caue
                                        // the schedule to start.
    volatile uint32_t time_trigger_us; // time (in "harp time" in microseconds)
                                       // when the schedule starts.
    volatile uint8_t schedule_port_state[6]; // {port mask (uint8_t),
                                             //  port state (uint8_t),
                                             //  microseconds (uint32_t)}
    // More app "registers" here.
} app_regs;

void set_port_direction(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // set gpio directions to outputs (1) or inputs (0).
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
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

void write_schedule_port_state(msg_t& msg)
{
    HarpCore::copy_msg_payload_to_register(msg);
    // FIXME: implement this.
    if (!HarpCore::is_muted())
        HarpCore::send_harp_reply(WRITE, msg.header.address);
}

// Define "specs" per-register
RegSpecs app_reg_specs[reg_count]
{
    {(uint8_t*)&app_regs.port_dir, sizeof(app_regs.port_dir), U8},
    {(uint8_t*)&app_regs.port_raw, sizeof(app_regs.port_raw), U8},
    {(uint8_t*)&app_regs.schedule_ctrl, sizeof(app_regs.schedule_ctrl), U8},
    {(uint8_t*)&app_regs.trigger_type, sizeof(app_regs.trigger_type), U8},
    {(uint8_t*)&app_regs.pin_trigger_mask, sizeof(app_regs.pin_trigger_mask), U8},
    {(uint8_t*)&app_regs.pin_trigger_state, sizeof(app_regs.pin_trigger_state), U8},
    {(uint8_t*)&app_regs.time_trigger_us, sizeof(app_regs.time_trigger_us), U32},
    {(uint8_t*)&app_regs.schedule_port_state, sizeof(app_regs.schedule_port_state), U8}
    // More specs here if we add additional registers.
};

RegFnPair reg_handler_fns[reg_count]
{
    {HarpCore::read_reg_generic, set_port_direction},
    {read_from_port, write_to_port},
    {HarpCore::read_reg_generic, write_schedule_ctrl},
    {HarpCore::read_reg_generic, write_trigger_type},
    {HarpCore::read_reg_generic, HarpCore::write_reg_generic},
    {HarpCore::read_reg_generic, HarpCore::write_reg_generic},
    {HarpCore::read_reg_generic, HarpCore::write_reg_generic},
    {HarpCore::read_reg_generic, write_schedule_port_state},
    // More handler function pairs here if we add additional registers.
};

void update_app_state()
{
    // Nothing to do?
}

void reset_app()
{
    // init all pins used as GPIOs.
    gpio_init_mask((0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE));
    // Reset PORT to all inputs. Reset DIR to all outputs.
    gpio_set_dir_masked((0xFF << PORT_DIR_BASE) || (0xFF << PORT_BASE),
                        (0xFF << PORT_DIR_BASE)); // 1 is output; 0 is input.
    gpio_put_masked((0xFF << PORT_DIR_BASE), 0); // Set DIR bits to all 0.
}

// Create Core.
HarpCApp& app = HarpCApp::init(who_am_i, hw_version_major, hw_version_minor,
                               assembly_version,
                               harp_version_major, harp_version_minor,
                               fw_version_major, fw_version_minor,
                               serial_number, "Cuttlefish",
                               &app_regs, app_reg_specs,
                               reg_handler_fns, reg_count, update_app_state,
                               reset_app);

// Core0 main.
int main()
{
#ifdef DEBUG
    stdio_uart_init_full(uart1, 921600, UART_TX_PIN, -1); // use uart1 tx only.
    printf("Hello, from an RP2040!\r\n");
#endif
    // Init Synchronizer.
    HarpSynchronizer::init(uart0, HARP_SYNC_RX_PIN);
    app.set_synchronizer(&HarpSynchronizer::instance());
    reset_app();
    while(true)
        app.run();
}
