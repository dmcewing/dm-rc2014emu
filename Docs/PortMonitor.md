# Module: Port Monitor

This is a debugging analysis monitor.  It attaches to all the available ports from 0x00 to 0xFF and just writes to the console when there is a read or write to that port.

This module was developed to investigate and find what ports the RomWBW was attempting to read/write to so I could deduce what hardware module I might want to attempt to emulator.

After implementing and when choosing the device on port 0xC0 (DSRTC) I discovered this is quite well documented in the config source files for the RomWBW. So isn't really needed, but is left in to show the ease of attaching to multiple ports.