#include <pico/stdlib.h>
#include <cstdio>
#include <cstdint>
#include <iterator>
#include <pulse_train_task.h>

inline constexpr uint32_t LED_PIN = 25;

// Create a pulse task.
pulse_events_t pulse_events[] = {{1, 0}, {0, 1'000'000}};
PulseTrainTask pulse_task(pulse_events, std::size(pulse_events), 2'000'000,
                          1 << LED_PIN, 0);


int main()
{
    stdio_usb_init();
    stdio_set_translate_crlf(&stdio_usb, false); // Don't replace outgoing chars.
    while (!stdio_usb_connected()){} // Block until connection to serial port.


    while(true)
        pulse_task.spin();
//printf("SYSTICK Register: %d\r\n",SYST_CVR);
}
