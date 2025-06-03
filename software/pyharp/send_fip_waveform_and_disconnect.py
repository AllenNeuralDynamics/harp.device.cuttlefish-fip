#!/usr/bin/env python3
from pyharp.device import Device, DeviceMode
from pyharp.messages import WriteU8HarpMessage, WriteU8ArrayMessage
from pyharp.messages import MessageType
from pyharp.messages import CommonRegisters as Regs
#import logging
from app_registers import AppRegs
from time import sleep
import serial.tools.list_ports

#logger = logging.getLogger()
#logger.setLevel(logging.DEBUG)
#logger.addHandler(logging.StreamHandler())
#logger.handlers[-1].setFormatter(
#    logging.Formatter(fmt='%(asctime)s:%(name)s:%(levelname)s: %(message)s'))

# Open serial connection with the first Valve Controller.
com_port = None
ports = serial.tools.list_ports.comports()
for port, desc, hwid in sorted(ports):
    if desc.startswith("cuttlefish-fip"):
        print("{}: {} [{}]".format(port, desc, hwid))
        com_port = port
        break
device = Device(com_port)

# Provision device with a square wave.
settings = \
(
    0b00000001,   # pwm_pin
    0.5, # pwm duty cycle
    10000., # pwm frequency (hz)
    0b00000010, # output mask
    1,      # events
    0,      # mute
    15350,  # DELTA1
    666,    # DELTA2
    600,    # DELTA3
    50     # DELTA4
)
data_fmt = "<LffLBBLLLL"

print("Disabling schedule.")
device.send(WriteU8HarpMessage(AppRegs.SetTasksState, 0).frame)
print("Clearing all tasks.")
device.send(WriteU8HarpMessage(AppRegs.RemoveAllLaserTasks, 1).frame)

print("Configuring device with FIP task.")
measurement = device.send(WriteU8ArrayMessage(AppRegs.AddLaserTask,
                                              data_fmt, settings).frame)
print("Enabling schedule")
device.send(WriteU8HarpMessage(AppRegs.SetTasksState, 1).frame)
sleep(1)

print("Disconnecting. Device should kill waveform in 3 seconds.")
device.disconnect()
