using Konamiman.Z80dotNet;
using Microsoft.Extensions.DependencyInjection;
using RC2014.EMU;
using RC2014.EMU.Module;
using RC2014VM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static ConfigurationEnum CONFIG = ConfigurationEnum.RC2014Pro;

        [STAThread]
        static void Main(string[] args)
        {
            //Console.TreatControlCAsInput = true;
            //ConsoleHelper.SetVT100Out();

            CURRENT.ResetVM();

            Application app = _app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Run();

        }

        private static void BuildServiceProvider(IModule[] modules)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<IZ80Processor, Z80Processor>();
            services.AddTransient<RC2014.EMU.RC2014>();
            
            services.AddTransient(module => modules);

            _serviceProvider = services.BuildServiceProvider();
        }

        internal void Step()
        {
            _VM.CPU.ExecuteNextInstruction();
        }

        public void ResetVM()
        {
            ResetVM(MachineConfigurations.GetConfigurations(CONFIG));
        }

        public void ResetVM(IModule[] configuration)
        {
            Stop();
            CancelThreads();
            BuildServiceProvider(configuration);
            InitVM();
            Start();
        }

        public void Stop() =>
            _VM?.Stop();

        public void Start() => 
            _tasks.Prepend(Task.Factory.StartNew(() => _VM.Start(), _cancellationToken));

        public void Resume() =>
            _tasks.Prepend(Task.Factory.StartNew(() => _VM.Resume(), _cancellationToken));

        public void Restart()
        {
            Console.Clear();
            _VM.Restart();
        }

        private static void InitVM()
        {
            CancelThreads();

            var vm = _VM = _serviceProvider.GetService<RC2014.EMU.RC2014>();

            _monitor?.SetVM(vm);

            var console = vm.Ports.First(p => p is IConsoleFeed) as IConsoleFeed;
            console.Initalise();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _tasks.Prepend(Task.Factory.StartNew(() => console.KeyboardHandler(_cancellationToken, HandleKey), _cancellationToken));

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
