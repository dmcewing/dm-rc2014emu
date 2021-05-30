# Module: PageableROM

## Description
This module emulates the [Pageable ROM mdoule](https://rc2014.co.uk/modules/pageable-rom/) from RC2014.

## Ports
None.

## Interrupts
None.

## Function
This module contains a specified amount of read only memory that is read from a ROM image.

The constructor can be used to specify the _fileName_, _page_ from the ROM, and the _end_ address to show. 
As the ROM is always loaded from **0x0000** the end is the size of the ROM image.  By default the 1st page, page = 0, and a size of 8 KiB is specified so only the filename is required.

The two following lines are therefore equivalent:
``` csharp
 var module1 = PageableROM(fileName, pageSelection, 0x1FFF);
 var module2 = PageableROM(fileName);
```
