# Modules

The RC2014 is constructed using pluggable modules.  This emulator is the same except that currently the pluggable modules need to be plugged in in code. I intend to make this software based eventually.

The modules that I have emulated so far are listed below.

## Memory Modules

These modules are by far the easiest.  They emulate the management of RAM and/or ROM for the computer.

### RAM32
This module emulates 32k of RAM starting at address 0x8000.  This is the main memory module for the smaller RC2014 computers.

### RAM64
This module emulates a full 64k of RAM.  By default it starts at 0x2000 and goes to 0xFFFF.  This allows a 8k ROM to be added in the first part of the address space.  The constructor for this allows you to configure this address space as you require.  To be even funkier, you can add mutiple modules if you desire in different ranges.

### PageableROM
This moudule emulates the standard RC2014 pageable ROM module.  It takes on its constructor a reference to the file location of a ROM image and the page of the ROM image to load.  This parameter is equivalent to the jumpers on the physical module.

### RAM512
This module which is big enough to have its [own documentation](RAM512.md), emulates 512kiB of RAM and 512kiB of ROM.  The ROM is loaded from an image as for the PageableROM. And each of the 8kiB pages can be switched by writing to an I/O port.

## I/O Modules
The Z80 processor accesses I/O devices by accessing special ports in the range 0x00 to 0xFF.  The modules in this section implement an I/O device in this range.

### PortMonitor

### BQRTC
This emulates a Real-time clock (RTC) based on the BQ4845 chip. This is a easy clock to emulate as it exposes the time values as a range of ports acting much like memory.  Note: Writing time has not been emulated and will be ignored.

### DSRTC
This RTC is more complicated as it works as the interface to the chip is a serial interface, with a clock driven by the CPU by writing values to the port. This means that the module needs to keep track of which bits are changing between each write to the port and manage state accordingly.

### SIO
I make no assertion that this module emulates the SIO chip at all beyond giving the appearance of it working to the CPU. Internally it tracks the keyboard buffer and redirects the output to the console setting ths rts and dts flags as necesary so the correct behaviour is observed by the programs I was experimenting with.


