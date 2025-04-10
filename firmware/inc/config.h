#ifndef CONFIG_H
#define CONFIG_H


/*
inline constexpr uint16_t HARP_DEVICE_ID = 0x057B; // FIXME: should be distinct from old cuttlefish.
inline constexpr uint16_t WHO_AM_I = HARP_DEVICE_ID;
inline constexpr uint8_t HW_VERSION_MAJOR = 0;
inline constexpr uint8_t HW_VERSION_MINOR = 0;
inline constexpr uint8_t ASSEMBLY_VERSION = 0;
inline constexpr uint8_t HARP_VERSION_MAJOR = 0;
inline constexpr uint8_t HARP_VERSION_MINOR = 0;
inline constexpr uint8_t FW_VERSION_MAJOR = 0;
inline constexpr uint8_t FW_VERSION_MINOR = 0;
inline constexpr uint16_t SERIAL_NUMBER = 0;
*/

#define DEBUG_UART (uart0)
#define DEBUG_UART_TX_PIN (0) // for printf-style debugging.

#define SYNC_UART (uart1)
#define HARP_SYNC_RX_PIN (5)
#define LED0 (24)
#define LED1 (25)

#define PORT_BASE (8)
#define PORT_DIR_BASE (16)


#define MAX_TASK_COUNT (8)

inline constexpr uint8_t MAX_QUEUE_SIZE = 32;

#endif // CONFIG_H
