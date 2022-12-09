using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.Module
{
    public class PortMonitor : IPort
    {
        public ushort[] HandledPorts { get; private set; }

        public PortMonitor()
        {
            List<ushort> ports = new List<ushort>();
            for (int i = 0; i < 256; i++)
            {
                ports.Add((ushort)i);
            }
            HandledPorts = ports.ToArray();
        }

        public byte GetData(ushort port)
        {
            Debug.WriteLine("Get from 0x{0:X2}", port);
            return 0;
        }

        public void SetData(ushort port, byte value)
        {
            Debug.WriteLine("Set to 0x{0:X2}: {1:X2}", port, value);
        }

        public void SaveState(Stream saveStream)
        {
        }

        public void LoadState(Stream loadStream)
        {
        }
    }
}
