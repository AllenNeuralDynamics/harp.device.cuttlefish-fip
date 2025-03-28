#include <pico/stdlib.h>
#include <cstdio>
#include <cstdint>
#include <iterator>
#include <pwm_task.h>

inline constexpr uint32_t LED_PIN = 25;

// Create a pulse task.
pwm_event_t pwm_events[] = {{0.5, 10'000, 0}, {1.0, 10'000, 500'000}};
pwm_event_t* pwm_event_ptrs[] = {&pwm_events[0], &pwm_events[1]};
PWMTask pwm_task(pwm_event_ptrs, std::size(pwm_events), 1'000'000, LED_PIN);


int main()
{
    stdio_usb_init();
    stdio_set_translate_crlf(&stdio_usb, false); // Don't replace outgoing chars.
    while (!stdio_usb_connected()){} // Block until connection to serial port.
    printf("Hello, world!\r\n");

    pwm_task.start();
    while(true)
        pwm_task.spin();
}
