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
SW_TRIGGER_REG = 37
SCHEDULE_CTRL_REG = 38

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
settings = \
(
    0,          # offset_us
    500000,     # on_time_us
    1000000,    # period_us
    (1 << 0),   # port_mask. 0 is device pin0.
    0,          # cycles
    False       # invert
)
data_fmt = "<LLLBLB"
print("Configuring device with PWM task.")
measurement = device.send(WriteU8ArrayMessage(PWM_TASK_REG,
                                              data_fmt, settings).frame)
print("Enabling task.")
device.send(WriteU8HarpMessage(SW_TRIGGER_REG, int(True)).frame)

sleep(3)

print("Disabling task.")
device.send(WriteU8HarpMessage(SCHEDULE_CTRL_REG, 1).frame)
