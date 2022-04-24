using RC2014.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM = RC2014.Core.RC2014;

namespace RC2014
{
    public class Emulator
    {
        private VM _VM;
        private bool _stopping = false;
        
        private readonly ConfigurationEnum machineType;

        public Emulator(ConfigurationEnum config)
        {
            machineType = config;
            InitVM();
        }

        public void InitVM()
        {
            InitVM(MachineConfigurations.GetConfigurations(machineType));
        }

        public void InitVM(IModule[] modules)
        {
            CancelThreads();

            _stopping = false;
            _VM = new VM(modules, HandleKey, IsStopping);
            _VM.Start();
        }

        public bool IsStopping() => _stopping;

        public bool IsRunning()
        {
            return _VM.CPU.State == Zem80.Core.ProcessorState.Running;
        }

        private void ResetVM()
        {
            CancelThreads();
            InitVM();
        }

        public void RunUntilStopped()
        {
            _VM.CPU.RunUntilStopped();
        }

        public void Restart()
        {
            Console.Clear();
            _VM.Restart();
        }

        public void Stop()
        {
            _stopping = true;
            _VM.Stop();
        }

        public void SaveState()
        {
            //string FileName = "State.bin";

            //Stream saveFileStream = File.Create(FileName);
            //BinaryFormatter serializer = new BinaryFormatter();
            //serializer.Serialize(saveFileStream, new StateCheck() { MachineType = MachineType });
            //foreach (IModule module in _VM.Modules)
            //{
            //    module.SaveState(serializer, saveFileStream);
            //}
            //saveFileStream.Close();
        }

        public void LoadState()
        {
            //string FileName = "State.bin";

            //using (Stream openFileStream = File.OpenRead(FileName))
            //{
            //    BinaryFormatter deserializer = new BinaryFormatter();

            //    StateCheck state = deserializer.Deserialize(openFileStream) as StateCheck;
            //    if (state.MachineType == MachineType)
            //    {
            //        foreach (IModule item in _VM.Modules)
            //        {
            //            item.LoadState(deserializer, openFileStream);
            //        }
            //    }
            //    else
            //    {
            //        _ = MessageBox.Show(string.Format("State is type of {0}, cannot be reinstated to {1}", state.MachineType, MachineType), "Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    }
            //    openFileStream.Close();
            //}
        }


        private void CancelThreads()
        {
            _VM?.Stop();
            //(_VM?.Ports.First(p => p is IConsoleFeed) as IConsoleFeed)?.Reset();
        }

        private bool HandleKey(ConsoleKeyInfo k)
        {
            bool handled = false;

            switch (k.Key)
            {
                case ConsoleKey.F12:
                    if (ConsoleModifiers.Shift == (ConsoleModifiers.Shift & k.Modifiers))
                        Stop();
                    else
                        ResetVM();
                    handled = true;
                    break;

                //case ConsoleKey.F11:
                //    Stop();
                //    handled = true;
                //    break;

                    //case ConsoleKey.F6:
                    //    if (_monitor == null)
                    //    {
                    //        CreateMonitor();
                    //    }
                    //    handled = true;
                    //    break;

                    //case 0:
                    //    handled = true;
                    //    break;
            }
            
            return handled;
        }
    }
}
