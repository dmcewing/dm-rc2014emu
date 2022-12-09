using RC2014.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        public Emulator(ConfigurationEnum config, bool loadState = false)
        {
            machineType = config;
            InitVM();
            if (loadState)
                LoadState();

            if (_VM == null)
                throw new ApplicationException("Error initalising VM");
            _VM.Start();
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
        }

        public bool IsStopping() => _stopping;

        public bool IsRunning()
        {
            return _VM.CPU.State == Zem80.Core.ProcessorState.Running;
        }

        private void ResetVM()
        {
            CancelThreads();
            
            while (IsRunning())
            { 
                Thread.Yield();
            }
            InitVM();
            _VM.Start();
        }

        public void RunUntilStopped()
        {
            _VM.CPU.RunUntilStopped();
        }

        public void Restart()
        {
            //Console.Clear();
            _VM.Restart();
        }

        public void Stop()
        {
            _stopping = true;
            _VM.Stop();
        }

        public void SaveState()
        {
            string FileName = "State.bin";

            Stream saveFileStream = File.Create(FileName);
            //serializer.Serialize(saveFileStream, new StateCheck() { MachineType = MachineType });
            foreach (IModule module in _VM.Modules)
            {
                module.SaveState(saveFileStream);
            }
            saveFileStream.Close();
        }

        public void LoadState()
        {
            string FileName = "State.bin";

            using (Stream openFileStream = File.OpenRead(FileName))
            {
                //StateCheck state = deserializer.Deserialize(openFileStream) as StateCheck;
                //if (state.MachineType == MachineType)
                //{
                    foreach (IModule item in _VM.Modules)
                    {
                        item.LoadState(openFileStream);
                    }
                //}
                //else
                //{
                //    _ = MessageBox.Show(string.Format("State is type of {0}, cannot be reinstated to {1}", state.MachineType, MachineType), "Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //}
                openFileStream.Close();
            }
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

                case ConsoleKey.F2:
                    if (ConsoleModifiers.Shift == (ConsoleModifiers.Shift & k.Modifiers))
                    {
                        SaveState();
                        handled = true;
                    }
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
