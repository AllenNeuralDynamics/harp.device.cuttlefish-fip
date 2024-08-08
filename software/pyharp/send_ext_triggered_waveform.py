#!/usr/bin/env python3
from pyharp.device import Device, DeviceMode
from pyharp.messages import WriteU8HarpMessage, WriteU8ArrayMessage
from pyharp.messages import MessageType
from pyharp.messages import CommonRegisters as Regs
from struct import pack, unpack
import logging
import os
from time import sleep, perf_counter

PORT_DIR_REG = 32
PORT_STATE = 33

PWM_TASK_REG = 34
ARM_EXT_TRIGGER = 35
EXT_TRIGGER_EDGE = 36

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
settings = \
(
    0,          # offset_us
    500000,     # on_time_us
    1000000,    # period_us
    (1 << 0),   # port_mask. 0 is device pin0.
    0,          # cycles. (0 = repeat forever.)
    False       # invert.
)
data_fmt = "<LLLBLB"
print("Configuring device with PWM task on pin 0.")
measurement = device.send(WriteU8ArrayMessage(PWM_TASK_REG,
                                              data_fmt, settings).frame)

print("Setting pin 0-6 as inputs. Setting pin 7 to output and HIGH.")
device.send(WriteU8HarpMessage(PORT_DIR_REG, int(0x80)).frame)
device.send(WriteU8HarpMessage(PORT_STATE, int(0x80)).frame)
print("Setting task to externally trigger from input on pin 3 on RISING input.")
device.send(WriteU8HarpMessage(ARM_EXT_TRIGGER, int(0x08)).frame)
device.send(WriteU8HarpMessage(EXT_TRIGGER_EDGE, int(0x08)).frame)
print("Waiting to see external trigger.")
try:
    while True:
        event_response = device._read()
        if event_response is not None:
            print()
            print(event_response)
except KeyboardInterrupt:
    print("disabling outputs.")
    device.send(WriteU8HarpMessage(PORT_STATE, int(0x00)).frame)
    print("Disabling task.")
    device.send(WriteU8HarpMessage(SCHEDULE_CTRL_REG, 1).frame)
