// See https://aka.ms/new-console-template for more information
using RC2014;
var emulator = new Emulator(MachineConfigurations.GetConfigurations(ConfigurationEnum.RC2014Pro));
do
{
    do
    {
        Thread.Yield();
    } while (emulator.IsRunning());

    //Pause for a bit... then check the running state again.  It might have been in the middle of a reset.
    Thread.Sleep(50);

} while (emulator.IsRunning());