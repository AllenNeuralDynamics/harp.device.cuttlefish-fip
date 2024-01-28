cmake_minimum_required(VERSION 3.13)

include(${PICO_SDK_PATH}/pico_sdk_init.cmake)

project(cuttlefish)

set(CMAKE_CXX_STANDARD 17)

# Enable try/catch exception interface.
#set(PICO_CXX_ENABLE_EXCEPTIONS 1)


# Compile for profiling/debugging/etc. Default: none enabled.
add_definitions(-DDEBUG)
#add_definitions(-DPROFILE_CPU)
#add_definitions(-DDEBUG_HARP_MSG_IN)
#add_definitions(-DDEBUG_HARP_MSG_OUT)

# initialize the Raspberry Pi Pico SDK
pico_sdk_init()

# Add Harp Core as separate library.
add_subdirectory(lib/harp.core.rp2040/firmware)
add_subdirectory(lib/etl build)

add_library(pwm_scheduler
    src/pwm_scheduler.cpp
)

add_library(pwm_task
    src/pwm_task.cpp
)

add_executable(${PROJECT_NAME}
    src/main.cpp
    src/cuttlefish_app.cpp
)

# Specify where to look for header files if they're not all in the same place.
#target_include_directories(${PROJECT_NAME} PUBLIC inc)
# Specify where to look for header files if they're all in one place.
include_directories(inc)

#set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fverbose-asm")

# Link libraries to the targets that need them.
target_link_libraries(pwm_scheduler PUBLIC pico_stdlib etl::etl)
target_link_libraries(pwm_task hardware_gpio pico_stdlib)
target_link_libraries(${PROJECT_NAME} PRIVATE etl::etl)
target_link_libraries(${PROJECT_NAME} PUBLIC pico_stdlib pwm_scheduler pwm_task
                      harp_core harp_sync harp_c_app)

# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${PROJECT_NAME})

if(DEBUG)
    message(WARNING "Debug printf() messages enabled from harp core to UART \
            with baud rate 921600.")
    pico_enable_stdio_uart(${PROJECT_NAME} 1)
    pico_enable_stdio_uart(pwm_scheduler 1)
endif()
