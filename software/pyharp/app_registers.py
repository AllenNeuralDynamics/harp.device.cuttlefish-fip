"""App registers for the cuttlefish-fip controller."""
from enum import IntEnum


class AppRegs(IntEnum):
    SetTasksState = 32
    AddLaserTask = 33
    RemoveLaserTask = 34
    RemoveAllLaserTasks = 35
    LaserTaskcount = 36
    RisingEdgeEvent = 37

    ReconfigureLaserTask0 = 38
    ReconfigureLaserTask1 = 39
    ReconfigureLaserTask2 = 40
    ReconfigureLaserTask3 = 41
    ReconfigureLaserTask4 = 42
    ReconfigureLaserTask5 = 43
    ReconfigureLaserTask6 = 44
    ReconfigureLaserTask7 = 45
