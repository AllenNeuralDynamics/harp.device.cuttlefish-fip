#ifndef CONFIG_H
#define CONFIG_H


#define WHO_AM_I (HARP_DEVICE_ID)
#define HW_VERSION_MAJOR (0)
#define HW_VERSION_MINOR (0)
#define ASSEMBLY_VERSION (0)
#define HARP_VERSION_MAJOR (0)
#define HARP_VERSION_MINOR (0)
#define FW_VERSION_MAJOR (0)
#define FW_VERSION_MINOR (0)
#define SERIAL_NUMBER (0)

#define DEBUG_UART (uart0)
#define DEBUG_UART_TX_PIN (0) // for printf-style debugging.

#define SYNC_UART (uart1)
#define HARP_SYNC_RX_PIN (5)
#define LED0 (24)
#define LED1 (25)

#define PORT_BASE (8)
#define PORT_DIR_BASE (16)

#define HARP_DEVICE_ID (0x057B) // FIXME: should be distinct from old cuttlefish.

#endif // CONFIG_H
