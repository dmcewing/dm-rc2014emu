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
        private Task _keyboardHandler;

        public Emulator(IModule[] modules)
        {
            InitVM(modules);
        }

        public void InitVM(IModule[] modules)
        {
            CancelThreads();

            _VM = new VM(modules);
            
            IConsoleFeed console = _VM.Ports.First(p => p is IConsoleFeed) as IConsoleFeed;
            console.Initalise();
            
            _keyboardHandler = new Task(() => { console.KeyboardHandler(HandleKey); });
            _keyboardHandler.Start();

            _VM.Start();
        }

        private void ResetVM()
        {
            Stop();
            CancelThreads();
            InitVM(_VM.Modules);
//            Start();
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
            (_VM?.Ports.First(p => p is IConsoleFeed) as IConsoleFeed)?.Reset();
        }

        private bool HandleKey(ConsoleKeyInfo k)
        {
            bool handled = false;

            switch (k.Key)
            {
                case ConsoleKey.F12:
                    ResetVM();
                    handled = true;
                    break;

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
