#include <core1_main.h>

void run_task_loop()
{
    // TODO: check for commands from core0.
    // TODO: consider replacing this with a proper scheduler.
    while true:
    {
        for (const auto& task : tasks)
            task.spin();
    }
}

// Core1 main.
void core1_main()
{
#if defined(DEBUG)
    printf("Hello from core1.\r\n");
#endif
    run_task_loop(); // blocks.
}
