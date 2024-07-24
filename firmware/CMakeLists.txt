cmake_minimum_required(VERSION 3.13)
find_package(Git REQUIRED)
execute_process(COMMAND "${GIT_EXECUTABLE}" rev-parse --short HEAD OUTPUT_VARIABLE COMMIT_ID OUTPUT_STRIP_TRAILING_WHITESPACE)
message(STATUS "Computed Git Hash: ${COMMIT_ID}")
add_definitions(-DGIT_HASH="${COMMIT_ID}") # Usable in source code.

include(${PICO_SDK_PATH}/pico_sdk_init.cmake)

project(cuttlefish-${COMMIT_ID})

set(CMAKE_CXX_STANDARD 17)

# Enable try/catch exception interface.
#set(PICO_CXX_ENABLE_EXCEPTIONS 1)


# Compile for profiling/debugging/etc. Default: none enabled.
#add_definitions(-DDEBUG) # Warning! initializing uart slows down core1 loop.
#add_definitions(-DPROFILE_CPU) # Warning! This slows down the core1 loop.
#add_definitions(-DDEBUG_HARP_MSG_IN)
#add_definitions(-DDEBUG_HARP_MSG_OUT)

# Configure the entire program to be copied from flash to RAM at the start.
# Since our binary size is small (<260KB), this is easier than marking every
# data structure and function definition called in core1 to run from RAM.
set(PICO_COPY_TO_RAM 1)

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

add_library(core1_main
    src/core1_main.cpp
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
target_link_libraries(pwm_task PUBLIC hardware_gpio pico_stdlib)
target_link_libraries(core1_main PRIVATE etl::etl)
target_link_libraries(core1_main PUBLIC pico_stdlib pwm_scheduler pwm_task)
target_link_libraries(${PROJECT_NAME} PUBLIC pico_stdlib core1_main
                      pico_multicore harp_core harp_sync harp_c_app)


# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${PROJECT_NAME})

if(DEFINED DEBUG)
    message(WARNING "Debug printf() messages enabled from harp core to UART \
            with baud rate 921600.")
    #pico_enable_stdio_uart(${PROJECT_NAME} 1)
    #pico_enable_stdio_uart(pwm_scheduler 1)
    pico_enable_stdio_uart(core1_main 1)
endif()
