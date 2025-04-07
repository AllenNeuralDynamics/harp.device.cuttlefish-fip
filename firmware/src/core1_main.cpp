#include <core1_main.h>

// Core1 main.
void core1_main()
{
#if defined(DEBUG)
    printf("Hello from core1.\r\n");
#endif
    setup_fip_schedule();

    run(); // blocks.
}
