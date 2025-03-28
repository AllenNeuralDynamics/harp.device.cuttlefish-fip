#include <pico/stdlib.h>
#include <cstdio>
#include <cstdint>
#include <fip_schedule.h>

inline constexpr uint32_t LED_PIN = 25;


int main()
{
    setup_fip_schedule();
//    stdio_usb_init();
//    stdio_set_translate_crlf(&stdio_usb, false); // Don't replace outgoing chars.
//    while (!stdio_usb_connected()){} // Block until connection to serial port.
//    printf("Hello, world!\r\n");

    while(true)
        run();
}
