#!/usr/bin/env python3
from pyharp.device import Device, DeviceMode
from pyharp.messages import WriteU8HarpMessage, WriteU8ArrayMessage, ReadU8HarpMessage
from pyharp.messages import MessageType
from pyharp.messages import CommonRegisters as Regs
from app_registers import AppRegs
from time import sleep
import serial.tools.list_ports
import threading
import sys
import collections

# Function to find and connect to the device
def find_device():
    ports = serial.tools.list_ports.comports()
    for port, desc, hwid in sorted(ports):
        if port.startswith("/dev/ttyUSB0") or port.startswith("/dev/ttyACM0") or port.startswith("COM5"):  
            return Device(port)
        elif desc.startswith("cuttlefish-fip"):
            return Device(port)
    raise Exception("Device not found.")


settings = (
        0b00000001,   # pwm_pin
        0.5,  # pwm duty cycle
        10000.,  # pwm frequency (hz)
        0b00000010,  # output mask
        1,      # events
        0,      # mute
        15350,  # DELTA1
        666,    # DELTA2
        600,    # DELTA3
        50     # DELTA4
    )


# Function to send FIP waveform settings
def send_fip_waveform(device):

    data_fmt = "<LffLBBLLLL"

    print("Disabling schedule.")
    device.send(WriteU8HarpMessage(AppRegs.SetTasksState, 0).frame)
    print("Clearing all tasks.")
    device.send(WriteU8HarpMessage(AppRegs.RemoveAllLaserTasks, 1).frame)

    print("Configuring device with FIP task.")
    device.send(WriteU8ArrayMessage(AppRegs.AddLaserTask, data_fmt, settings).frame)

    print("Enabling schedule.")
    device.send(WriteU8HarpMessage(AppRegs.SetTasksState, 1).frame)

    sleep(3)

    print("Disabling schedule.")
    device.send(WriteU8HarpMessage(AppRegs.SetTasksState, 0).frame)


# Function to listen for rising edge events
def listen_for_events(device):
    print("Listening for rising edge events from core 0...")
    event_times = collections.deque(maxlen=10)  # Store last 10 timestamps
    counter = 0

    while True:
        messages = device.get_events()
        for message in messages:
            # Assume message contains a 'timestamp' field in seconds
            if message._timestamp is not None:
                timestamp = message._timestamp
                event_times.append(timestamp)
                counter += 1
                if len(event_times) >= 2:
                    delta = event_times[-1] - event_times[-2]
                    if counter % 2 == 0:
                        nominal = settings[8]
                        print(f"Rising edge at {timestamp:.6f} s, Δt = {delta * 1e6:.2f} µs, nominal = {nominal} µs, error = {abs(delta * 1e6 - nominal):.2f} µs")
                    else:
                        nominal = settings[6] + settings[7] + settings[9]
                        print(f"Rising edge at {timestamp:.6f} s, Δt = {delta * 1e6:.2f} µs, nominal = {nominal} µs, error = {abs(delta * 1e6 - nominal):.2f} µs")
                else:
                    print(f"First Rising edge at {timestamp} us")

            print()


if __name__ == "__main__":
    try:
        device = find_device()
        device.info()

        # Create and start a thread for event listening
        event_thread = threading.Thread(target=listen_for_events, args=(device,))
        event_thread.daemon = True  # Thread will exit when main program exits
        event_thread.start()

        send_fip_waveform(device)

        sleep(2)  # Allow some time for events to be processed
        sys.exit(0)

    except Exception as e:
        print("An error occurred:", e)
