#!/usr/bin/env python3
from pyharp.device import Device
from pyharp.messages import WriteU8HarpMessage, WriteU8ArrayMessage, ReadU8HarpMessage
from pyharp.messages import MessageType
from pyharp.messages import CommonRegisters as Regs
from app_registers import AppRegs
from time import sleep
import serial.tools.list_ports
import sys


# Function to find and connect to the device
def find_device():
    ports = serial.tools.list_ports.comports()
    for port, desc, hwid in sorted(ports):
        if port.startswith("/dev/ttyUSB0") or port.startswith("/dev/ttyACM0") or port.startswith("COM5"):  
            return Device(port)
        elif desc.startswith("cuttlefish-fip"):
            return Device(port)
    raise Exception("Device not found.")


# Function to send FIP waveform settings
def configure_tasks(device):
    settings = [
        (
            0b00000001,   # pwm_pin
            0.25, # pwm duty cycle
            10000., # pwm frequency (hz)
            0b00000010, # output mask
            1,      # events
            0,      # mute
            15350,  # DELTA1
            666,    # DELTA2
            600,    # DELTA3
            50     # DELTA4
        ),

        (
            0b00000100,   # pwm_pin
            0.75, # pwm duty cycle
            10000., # pwm frequency (hz)
            0b00001000, # output mask
            1,      # events
            0,      # mute
            15350,  # DELTA1
            666,    # DELTA2
            600,    # DELTA3
            50     # DELTA4
        ),

        (
            0b00010000,   # pwm_pin
            0.5, # pwm duty cycle
            10000., # pwm frequency (hz)
            0b00100000, # output mask
            1,      # events
            0,      # mute
            15350,  # DELTA1
            666,    # DELTA2
            600,    # DELTA3
            50     # DELTA4
        ),

        (
            0b01000000,   # pwm_pin
            0.25, # pwm duty cycle
            10000., # pwm frequency (hz)
            0b10000000, # output mask
            1,      # events
            0,      # mute
            15350,  # DELTA1
            666,    # DELTA2
            600,    # DELTA3
            50     # DELTA4
        )
    ]

    data_fmt = "<LffLBBLLLL"

    print("\nDisabling schedule.")
    device.send(WriteU8HarpMessage(AppRegs.EnableTaskSchedule, 0).frame)
    print("\nClearing all tasks.")
    device.send(WriteU8HarpMessage(AppRegs.RemoveAllLaserTasks, 1).frame)

    check_configured_tasks(device)

    print("\n########################################")
    print("Configuring device with 4 FIP tasks.")
    device.send(WriteU8ArrayMessage(AppRegs.AddLaserTask, data_fmt, settings[0]).frame)
    device.send(WriteU8ArrayMessage(AppRegs.AddLaserTask, data_fmt, settings[1]).frame)
    device.send(WriteU8ArrayMessage(AppRegs.AddLaserTask, data_fmt, settings[2]).frame)
    device.send(WriteU8ArrayMessage(AppRegs.AddLaserTask, data_fmt, settings[3]).frame)
    check_configured_tasks(device)

    print("\n########################################")
    print("Reconfiguring task 1.")
    device.send(WriteU8ArrayMessage(AppRegs.ReconfigureLaserTask1, data_fmt, settings[3]).frame)
    check_configured_tasks(device)

    print("\n########################################")
    print("Removing task 1.")
    device.send(WriteU8HarpMessage(AppRegs.RemoveLaserTask, 1).frame)
    check_configured_tasks(device)

    print("\n########################################")
    print("Reconfiguring task 3.")
    # Should return an error since task 3 is not configured anymore
    print(device.send(WriteU8ArrayMessage(AppRegs.ReconfigureLaserTask3, data_fmt, settings[0]).frame))
    check_configured_tasks(device)


# Function to check configured tasks
def check_configured_tasks(device):

    print("Checking configured tasks...")
    print("Task count: ", device.send(ReadU8HarpMessage(AppRegs.LaserTaskcount).frame).payload_as_int())
    print("----------------------")

    task0 = device.send(ReadU8HarpMessage(AppRegs.ReconfigureLaserTask0).frame)
    print("TASK 0 payload: ", task0.payload)
    print("----------------------")

    task1 = device.send(ReadU8HarpMessage(AppRegs.ReconfigureLaserTask1).frame)
    print("TASK 1 payload: ", task1.payload)
    print("----------------------")

    task2 = device.send(ReadU8HarpMessage(AppRegs.ReconfigureLaserTask2).frame)
    print("TASK 2 payload: ", task2.payload)
    print("----------------------")

    task3 = device.send(ReadU8HarpMessage(AppRegs.ReconfigureLaserTask3).frame)
    print("TASK 3 payload: ", task3.payload)
    print("----------------------")


if __name__ == "__main__":
    try:
        device = find_device()
        device.info()

        configure_tasks(device)

        sys.exit(0)

    except Exception as e:
        print("An error occurred:", e)
