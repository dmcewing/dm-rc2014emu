# RC2014EMU

## Purpose
This project is a project aimed at me learning about the RC2014 hardware and modules without needing to spend copious amounts of money purchasing modules or test equipment to debug faulty soldering.  That being said, I thought this project contains a number of useful code other people might want to contribute to or learn from themselves.

The [RC2014](http://www.rc2014.co.uk) computer is a Homebrewed Z80 based computer.  Purchasable as a kit, highly configurable and with a reasonably large community for support.

## Project Structure
This project has two main assemblies.  **RC2014.EMU** which contains the emulation components, modules, CPU control etc.  and **RC2014VM** which is the main program and holds the Monitor, Computer configuration, and Console emulation.

### Modular
While not yet externally configurable the Emulator can be configured like the RC2014 by modifying the _IModules_ arrays in the _MachineConfigurations.cs_ file and pointing to that array by setting the static _CONFIG_ variable in the _Program.cs_ file.  I intend to make this software configurable in the future, but have been distracted by implementing some of the modules.

A [list of modules](Docs/ListOfModules.md) included in the emulator is avaialble.

### Monitor
To access the monitor for the CPU press **F6** in the console once it is running.  This key may change in the future but for now it doesn't appear to conflict with any other use.

