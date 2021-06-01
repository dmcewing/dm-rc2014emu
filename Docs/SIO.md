# Module: SIO

## Description
This module emulates the [Dual Serial Module SIO/2](https://rc2014.co.uk/modules/dual-serial-module-sio2/) board, kind of.  The module only emulates enough for Channel A serial port to be read and written to through the console.  Channel B control port mirrors the A control port, and no data is ever available on Channel B.

***NOTE:*** _This module only implements what is needed to get it working.  I have no idea if it is actually emulating the board correctly or not._

## Ports

| Channel | Function | Port   |
|---------|----------|--------|
|    A    |  Control |  0x80  |
|    A    |  Data    |  0x81  |
|    B    |  Control |  0x82  |
|    B    |  Data    |  0x83  |

## Interrupts

* _IntLine_ is active when there are bytes in the buffer.
* _ValueOnDataBus_ is always zero.

## Function
The main chip on the SIO board is a [Z84C4206PEG](https://www.alldatasheet.com/view.jsp?Searchword=Z84C4206PEG). This chip contains two RS232 serial interfaces.  The emulator however is concerned  ot with emulating a serial port but providing a terminal interface, therefore the emulation concerns itself with only channel A, and requires a console to be passed to it.

The console is passed via an interface so could in reality be seperate text readers and writers. However for convience some control codes are sent direct to the console violating this interface. (todo: fix it)

The SIO contains three read registers for Channel B and two read registeres for Channel A that can be read to obtain the status infromation.  

To read the conttents of a selected read register other than RR0 the system program must first write the pointer byte to WR0 in exactly the same was as a write register operation.  Then, by executing a read instruction, the contents of the addressed read register can be read by the CPU.

