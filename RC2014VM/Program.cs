using Konamiman.Z80dotNet;
using Microsoft.Extensions.DependencyInjection;
using RC2014.EMU;
using RC2014.EMU.Module;
using RC2014VM.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RC2014VM.UI
{
    class Program
    {
        internal static ServiceProvider _serviceProvider;
        internal static Application _app;

        private static RC2014.EMU.RC2014 _VM;
        private static MonitorWindow _monitor;

        private static CancellationTokenSource _cancellationTokenSource;
        private static CancellationToken _cancellationToken;

        private static List<Task> _tasks = new List<Task>();
        public static Program CURRENT = new Program();

        public static ConfigurationEnum MachineType = ConfigurationEnum.RC2014Pro;

        [STAThread]
        static void Main(string[] args)
        {
            CURRENT.ResetVM();

            Application app = _app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _ = app.Run();

        }

        private static void BuildServiceProvider(IModule[] modules)
        {
            ServiceCollection services = new ServiceCollection();
            _ = services.AddTransient<IZ80Processor, Z80Processor>()
                    .AddTransient<RC2014.EMU.RC2014>()
                    .AddTransient(module => modules);

            _serviceProvider = services.BuildServiceProvider();
        }

        internal void Step()
        {
            _ = _VM.CPU.ExecuteNextInstruction();
        }

        public void ResetVM()
        {
            ResetVM(MachineConfigurations.GetConfigurations(MachineType));
        }

        public void ResetVM(ConfigurationEnum machineType)
        {
            MachineType = machineType;
            ResetVM();
        }

        private void ResetVM(IModule[] configuration)
        {
            Stop();
            CancelThreads();
            BuildServiceProvider(configuration);
            InitVM();
            Start();
        }

        public void Stop()
        {
            _VM?.Stop();
        }

        public void Start()
        {
            _ = _tasks.Prepend(Task.Factory.StartNew(() => _VM.Start(), _cancellationToken));
        }

        public void Resume()
        {
            _ = _tasks.Prepend(Task.Factory.StartNew(() => _VM.Resume(), _cancellationToken));
        }

        public void Restart()
        {
            Console.Clear();
            _VM.Restart();
        }

        [Serializable]
        public class StateCheck
        {
            public ConfigurationEnum MachineType { get; set; }
        }

        public void SaveState()
        {
            string FileName = "State.bin";

            Stream saveFileStream = File.Create(FileName);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(saveFileStream, new StateCheck() { MachineType = MachineType });
            foreach (IModule module in _VM.Modules)
            {
                module.SaveState(serializer, saveFileStream);
            }
            saveFileStream.Close();
        }

        public void LoadState()
        {
            string FileName = "State.bin";

            using (Stream openFileStream = File.OpenRead(FileName))
            {
                BinaryFormatter deserializer = new BinaryFormatter();

                StateCheck state = deserializer.Deserialize(openFileStream) as StateCheck;
                if (state.MachineType == MachineType)
                {
                    foreach (IModule item in _VM.Modules)
                    {
                        item.LoadState(deserializer, openFileStream);
                    }
                }
                else
                {
                    _ = MessageBox.Show(string.Format("State is type of {0}, cannot be reinstated to {1}", state.MachineType, MachineType), "Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                openFileStream.Close();
            }
        }

        private static void InitVM()
        {
            CancelThreads();

            var vm = _VM = _serviceProvider.GetService<RC2014.EMU.RC2014>();

            _monitor?.SetVM(vm);

            IConsoleFeed console = vm.Ports.First(p => p is IConsoleFeed) as IConsoleFeed;
            console.Initalise();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _ = _tasks.Prepend(Task.Factory.StartNew(() => console.KeyboardHandler(_cancellationToken, HandleKey), _cancellationToken));

        }

        private static void CancelThreads()
        {
            _VM?.Stop();
            if (_cancellationTokenSource != null && !_cancellationToken.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();

                try
                {
                    Task.WaitAll(_tasks.ToArray());
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                    _tasks.Clear();
                    _cancellationTokenSource = null;
                }
            }
            Console.Clear();
            (_VM?.Ports.First(p => p is IConsoleFeed) as IConsoleFeed)?.Reset();
        }

        private static void _monitor_Closed(object sender, EventArgs e)
        {
            _monitor = null;
        }

        private static void CreateMonitor()
        {
            _ = _app.Dispatcher.BeginInvoke(new Action(() =>
              {
                  _monitor = new MonitorWindow(CURRENT);
                  _monitor.SetVM(_VM);
                  _monitor.Closed += _monitor_Closed;
                  _monitor.Show();
              }));
        }

        private static bool HandleKey(ConsoleKeyInfo k)
        {
            bool handled = false;

            switch (k.Key)
            {
                case ConsoleKey.F12:
                    CURRENT.ResetVM();
                    handled = true;
                    break;
                    
                case ConsoleKey.F6:
                    if (_monitor == null)
                    {
                        CreateMonitor();
                    }
                    handled = true;
                    break;

                //case 0:
                //    handled = true;
                //    break;
            }
            return handled;
        }
    }
}
