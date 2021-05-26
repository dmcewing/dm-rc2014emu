# Module: DSRTC

## Description
This module emulates the [DS1302-RTC](https://github.com/electrified/rc2014-ds1302-rtc) module. The main chip of this module is the DS1302 which has a serial interface exposed through the data bus. It therefore made the implementation tricky, but fun, module to emulate.

## Ports
This module acts on port **0xC0**.

## Interrupts
None.

## Further Notes
* Datasheet - https://datasheets.maximintegrated.com/en/ds/DS1302.pdf
* Data is read in serially through the I/O Port and buffered internally by the DS1302 until the complete command byte and corresponding information is recieved and/or transmitted.
* The DS1302 has 9 registers and 32 bytes of NVRAM memory which can be read/written individually or in burst mode.  This module emulates those, but ignores writes of the time, that is you can write to them, but it won't udpate the system time, or compensate.  A read of the time will always return the current system time.
* I attempted to port a C++ emulator for this module, but it didn't work in C#.  After a couple of weeks understanding how it was meant to be operating I did my own implementation from scratch in about three hours.

### Port interface.
The control wires / port interface are defined as such:

| 7      | 6       | 5      | 4      | 3 | 2 | 1 | 0 |
|--------|---------|--------|--------|---|---|---|---|
| RTC_IN | RTC_CLK | RTC_WE | RTC_CE |   |   |   |   |

