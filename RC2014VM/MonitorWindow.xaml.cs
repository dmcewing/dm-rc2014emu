using Konamiman.Z80dotNet;
using RC2014.EMU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RC2014VM.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        private RC2014.EMU.RC2014 rc2014;
        private bool stop = false;

        private readonly Program program;

        public MonitorWindow()
        {
            InitializeComponent();
        }

        internal MonitorWindow(Program p)
        {
            program = p;
            InitializeComponent();
        }

        private List<string> GetConfigurationNames()
        {
            Type t = typeof(ConfigurationEnum);
            return t.GetEnumNames().ToList();
        }
        
        public void SetVM(RC2014.EMU.RC2014 vm)
        {
            rc2014 = vm;
            stkCPU.Dispatcher.Invoke(() =>
            {
                stkCPU.DataContext = vm.CPU;
            });
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (Enum.TryParse((string)cboCofiguration.SelectedItem, out ConfigurationEnum config))
            {
                program.ResetVM(config);
            }
        }

        private void Step_Click(object sender, RoutedEventArgs e)
        {
            program.Step();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            stop = !stop;
            if (!stop)
            {
                Stop.Content = "Stop";
                program.Resume();
            }
            else
            {
                Stop.Content = "Continue";
                program.Stop();
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            cboCofiguration.ItemsSource = GetConfigurationNames();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cboCofiguration.SelectedItem = Program.MachineType.ToString();
        }

        private void SaveState_Click(object sender, RoutedEventArgs e)
        {
            program.SaveState();
        }

        private void LoadState_Click(object sender, RoutedEventArgs e)
        {
            program.LoadState();
        }
    }
}
