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
PORT_REG = 33

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

print("Configuring TTL pins 0, 1, 2, 3.")
device.send(WriteU8HarpMessage(PORT_DIR_REG, int(0x0F)).frame)
sleep(1)
for i in range(3):
    print("Writing: 0x0F", end = " ")
    reply = device.send(WriteU8HarpMessage(PORT_REG, int(0x0F)).frame)
    print(f" Read back: {hex(reply.payload[0])}")
    sleep(0.5)
    print("Writing: 0x00", end=" ")
    reply = device.send(WriteU8HarpMessage(PORT_REG, int(0x00)).frame)
    print(f" Read back: {hex(reply.payload[0])}")
    sleep(0.5)
