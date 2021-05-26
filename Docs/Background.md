# Background

One day I realised that I spendt around 3 hours of my day on public transport and I could efficiently use that time to do some programming on a pet project learning stuff I otherwise do not have the time for.  While in transit I would not have access to the internet in general, but material could be gather prior to the trip, and/or I could use my phone as a hotspot for the occasional lookups and queries on line if I needed to do some deeper research.

The pet project I decided on was to emulate the RC2014 computer.  Why the RC2014?  Well, it is a simple modular design and I had purchased a RC2014 Pro to put together during lockdown but I have yet to get it functioning.  I thought if I can get an emulator working at least I explore how it works (which for me is the fun part) and get to the point of perhaps learning some assembly for the Z80. (I'm yet to get there, I've been too busy learning other stuff).

My first thought was to build an emulator for the RC2014 Bus which I could then plug modules representing each of the cards. I completed that along with a clock module, 32K Memory module and Paged ROM module.  However, when implementing the CPU card it became apparent that this approach was flawed as any reasonable CPU emulator needed better access to control the clock cycles.

As the aim was not for an electrical level emulation but a more functional level I pivoted the design so that the base computer contains the CPU, clock and "bus" modules together, for which additional modules can be added.

## CPU Selection
Obviously I wanted a Z80 emulator and after much searching through GitHub I selected Konamiman's Z80dotNet emulator, as it was written in C# sharp, open source, so I could include it by code if I needed, and was highly pluggable with interfaces.  This meant that my core VM was rather easy as all I needed to do was implement a couple of small interfacing routines to iterate through the plugged in modules when required.

I ended up including the source code directly in the module during investigations to see if I could report the current executing instruction or similar in the monitor windows.  I've yet to implement that, so at the time of writing a reference to Konamiman's NuGet package is all the only CPU library dependency required.

Unfortunately the inclusion of this library as restricted the code to .NET 4.x which while disappointing that it wouldn't be a cross platform emulator, without the help of other emulators, it became helpful when adding forms for monitoring, and VT-100/ANSI terminal support.

## Monitoring
Originally I was writing the machine as a full WPF application, however, as the RC2014 is effectively a console based computer it made more sense to use a console app instead.  I've broken good practice though in that this is a hybrid app that functions as a console but on pressing the monitoring hot key (currently F6) a WPF app opens up.

## Emulating a console
The first big tackle, the memory modules, and CPU being rather straight forward to implement, was the SIO module.  This required port reading and writing as well as console output and keyboard interface to be implemented as well.  While the SIO module functions sufficiently for this emulator, I have only implemented enough functionality for it to function.  I make no claim to it actually being a true emulator of any SIO module.  It is really a specific implementation with enough capability to allow the console window to be attached as an I/O device.

To add ANSI support to the console I have made use of the Windows 10 support for ANSI in the console (Yes Microsoft finally added ANSI support to the console). To switch the console to ANSI mode it is neccessary to call a couple of Win32 Kernal API's this is one significant benefit of being a console first application.


## TODOs
- Monitoring
  - Current processing command monitoring.
  - Memory monitoring
- Save and Load state.
- Reset functionality isn't working correctly in all cases.
- Change the emulated modules.
- The console only supports ANSI on the outbound, needs to support ANSI in as well.
- Keyboard intercept routine needs to be moved to it's own class

