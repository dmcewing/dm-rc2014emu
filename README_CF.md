To build a disk image

    #  ./makedisk 1 my.cf
    #  dd if=cf-image of=my.cf bs=512 seek=2 conv=notrunc

In other words the IDE disk format has a 1K header that holds
meta-data and the virtual identify block.

Compact flash images can be found at

https://github.com/RC2014Z80/RC2014/tree/master/CPM