cmake_minimum_required(VERSION 3.13)
find_package(Git REQUIRED)
execute_process(COMMAND "${GIT_EXECUTABLE}" rev-parse --short HEAD OUTPUT_VARIABLE COMMIT_ID OUTPUT_STRIP_TRAILING_WHITESPACE)
message(STATUS "Computed Git Hash: ${COMMIT_ID}")
add_definitions(-DGIT_HASH="${COMMIT_ID}") # Usable in source code.


include(${PICO_SDK_PATH}/pico_sdk_init.cmake)

project(test-fip-schedule)

set(CMAKE_CXX_STANDARD 17)

# initialize the Raspberry Pi Pico SDK
pico_sdk_init()

add_subdirectory(../../lib/rp2040.pwm build/rp2040-pwm)
add_subdirectory(../../lib/etl build/etl)

add_library(laser_fip_task
    ../../src/laser_fip_task.cpp
)

add_executable(${PROJECT_NAME}
    main.cpp
    ../../src/fip_schedule.cpp
)

include_directories(../../inc)

#set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fverbose-asm")

# Link libraries to the targets that need them.
target_link_libraries(laser_fip_task rp2040_pwm)
target_link_libraries(${PROJECT_NAME}
    hardware_gpio pico_stdlib laser_fip_task etl::etl)

# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${PROJECT_NAME})
pico_enable_stdio_usb(${PROJECT_NAME} 1)
