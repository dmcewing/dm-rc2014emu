# Module: Compact Flash

## Description
This module is an emulator for the compact flash card module.  The code is largely a translation of the Etched Pixels CF module.

## Ports
This module acts on ports **0x10** to **0x1F**.  (NB: Not all ports are used.)

## Interrupts
TBC

## Further Notes

The module inself inherits from the `ATAPIBase` class which enables the IDE control information
to be reused between this module and the IDE modules when they are developed.

### Control Block Registers

 | REGISTER				| PORT	| DIR	| DESCRIPTION					|
 |----------------------|-------|-------|-------------------------------|
 | IDE_IO_ALTSTAT		| 0x0E	| R		| ALTERNATE STATUS REGISTER		|
 | IDE_IO_CTRL			| 0x0E	| W		| DEVICE CONTROL REGISTER		|
 | IDE_IO_DRVADR		| 0x0F	| R		| DRIVE ADDRESS REGISTER		|
 

### Command Block Registers 

 | REGISTER				| PORT	| DIR	| DESCRIPTION					|
 |----------------------|-------|-------|-------------------------------|
 | IDE_IO_DATA			| 0x00	| R/W	| DATA INPUT/OUTPUT				|
 | IDE_IO_ERR			| 0x01	| R		| ERROR REGISTER				|
 | IDE_IO_FEAT			| 0x01	| W		| FEATURES REGISTER				|
 | IDE_IO_COUNT			| 0x02	| R/W	| SECTOR COUNT REGISTER			|
 | IDE_IO_SECT			| 0x03	| R/W	| SECTOR NUMBER REGISTER		|
 | IDE_IO_CYLLO			| 0x04	| R/W	| CYLINDER NUM REGISTER (LSB)	|
 | IDE_IO_CYLHI			| 0x05	| R/W	| CYLINDER NUM REGISTER (MSB)	|
 | IDE_IO_DRVHD			| 0x06	| R/W	| DRIVE/HEAD REGISTER			|
 | IDE_IO_LBA0*			| 0x03	| R/W	| LBA BYTE 0 (BITS 0-7)			|
 | IDE_IO_LBA1*			| 0x04	| R/W	| LBA BYTE 1 (BITS 8-15)		|
 | IDE_IO_LBA2*			| 0x05	| R/W	| LBA BYTE 2 (BITS 16-23)		|
 | IDE_IO_LBA3*			| 0x06	| R/W	| LBA BYTE 3 (BITS 24-27)		|
 | IDE_IO_STAT			| 0x07	| R		| STATUS REGISTER				|
 | IDE_IO_CMD			| 0x07	| W		| COMMAND REGISTER (EXECUTE)	|
 
 \* LBA0-3 ARE ALTERNATE DEFINITIONS OF SECT, CYL, AND DRVHD PORTS

 ### Supported Commands
 The following commands have been implemented (from `ATAPIBase`)

| Command | Value | Description |
|---|---|---|
| UNKNOWN | 0x00 |
| CMD_EDD | 0x90 |
| CMD_IDENTIFY | 0xEC|
| CMD_INTPARAMS | 0x91|
| CMD_READ | 0x20|
| CMD_READ_NR | 0x21 |
| CMD_SETFEATURES | 0xEF |
| CMD_VERIFY | 0x40 |
| CMD_VERIFY_NR | 0x41 |
| CMD_WRITE | 0x30 | 
| CMD_WRITE_NR | 0x31 |
| CMD_CALIB | 0x10 | actually all 0x1? range
| CMD_SEEK | 0x70 | actually all 0x7? range
