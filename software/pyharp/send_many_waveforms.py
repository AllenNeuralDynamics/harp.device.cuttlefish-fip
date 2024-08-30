#!/usr/bin/env python3
from pyharp.device import Device, DeviceMode
from pyharp.messages import WriteU8HarpMessage, WriteU8ArrayMessage
from pyharp.messages import MessageType
from pyharp.messages import CommonRegisters as Regs
from struct import pack, unpack
import logging
import os
from time import sleep, perf_counter

PWM_TASK_REG = 34
SW_TRIGGER_REG = 39
SCHEDULE_CTRL_REG = 41

#logger = logging.getLogger()
#logger.setLevel(logging.DEBUG)
#logger.addHandler(logging.StreamHandler())
#logger.handlers[-1].setFormatter(
#    logging.Formatter(fmt='%(asctime)s:%(name)s:%(levelname)s: %(message)s'))

# Open the device and print the info on screen
# Open serial connection and save communication to a file
if os.name == 'posix': # check for Linux.
    #device = Device("/dev/harp_device_00", "ibl.bin")
    device = Device("/dev/ttyACM0", "ibl.bin")
else: # assume Windows.
    device = Device("COM95", "ibl.bin")

# Provision device with a square wave.
# offset_us, on_time_us, period_us, port_mask, cycles, invert.
pwm_task_settings = (
    (0, 500, 1000,      (1 << 0), 0, False),
    (0, 1000, 2500,     (1 << 1), 0, False),
    (0, 350, 700,       (1 << 2), 0, False),
    (0, 5000, 10000,    (1 << 3), 0, False),
    (0, 10000, 20000,   (1 << 4), 0, False),
    (0, 7500, 15000,    (1 << 5), 0, False),
    (0, 2000, 4000,     (1 << 6), 0, False),
    (0, 50000, 100000,  (1 << 7), 0, False)
)
#pwm_task_settings = (
#    (0, 1000000, 2000000, (1 << 0), 0, False),
#    (0, 100000, 1000000, (1 << 1), 0, False),
#    (0, 400000, 1000000, (1 << 2), 0, False),
#    (0, 300000, 1000000, (1 << 3), 0, False),
#    (0, 20000, 100000, (1 << 4), 0, False),
#    (0, 60000, 100000, (1 << 5), 0, False),
#    (0, 70000, 100000, (1 << 6), 0, False),
#    (0, 80000, 100000, (1 << 7), 0, False)
#)
data_fmt = "<LLLBLB"

print("Configuring device with PWM task.")
for setting in pwm_task_settings:
    device.send(WriteU8ArrayMessage(PWM_TASK_REG, data_fmt, setting).frame)

print("Enabling many tasks.")
device.send(WriteU8HarpMessage(SW_TRIGGER_REG, int(True)).frame)

sleep(5)

print("Disabling all tasks.")
device.send(WriteU8HarpMessage(SCHEDULE_CTRL_REG, 1).frame)
