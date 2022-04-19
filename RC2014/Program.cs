// See https://aka.ms/new-console-template for more information
using RC2014;
var emulator = new Emulator(MachineConfigurations.GetConfigurations(ConfigurationEnum.RC2014Plus64k));
do
{
    Thread.Yield();
} while (true);