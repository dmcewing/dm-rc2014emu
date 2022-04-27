// See https://aka.ms/new-console-template for more information
using RC2014;
var emulator = new Emulator(ConfigurationEnum.RC2014ProCF);
do
{
    Thread.Yield();
} while (!emulator.IsStopping());