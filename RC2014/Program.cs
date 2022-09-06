// See https://aka.ms/new-console-template for more information
using RC2014;
bool loadState = false;
if (File.Exists("State.bin"))
{
    //Prompt load state?:
    Console.WriteLine("Reload state [Y/n]?");
    string? response = Console.ReadLine();
    if (string.IsNullOrEmpty(response)
        || response.ToLowerInvariant().Contains("y"))
        loadState = true;
}
var emulator = new Emulator(ConfigurationEnum.RC2014ProCF, loadState);
do
{
    Thread.Yield();
} while (!emulator.IsStopping());