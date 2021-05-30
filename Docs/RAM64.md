# Module: RAM64

## Description
This module emulates upto a full 64k of memory depending on what is specified on the constructor.

By default it emulates between **0x2000** and **0xFFFF** allowing an 8k ROM to be loaded in the first 8k.

## Constructor
The default constructor loads the start and end addresses as above, if a different amount of memory or range of memory is required then the start and end address can be specified on the constructor as follows:

``` csharp
var ram64 = new RAM64(startAddress, endAddress);
```