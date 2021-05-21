using Konamiman.Z80dotNet;
using RC2014.EMU.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RC2014.EMU
{
    public class RC2014 : IRC2014
    {
        public readonly IZ80Processor CPU;
        public readonly IPort[] Ports; // { get; private set; }

        private bool _stopRequested = false;

        public RC2014(IZ80Processor cpu, IModule[] modules)
        {
            IPort[] ports = modules.Where(m => m is IPort).Cast<IPort>().ToArray();
            IMemoryBank[] memoryBanks = modules.Where(m => m is IMemoryBank).Cast<IMemoryBank>().ToArray();

            CPU = cpu;
            this.Ports = ports;
            CPU.Registers = new NotifiedZ80Registers();
            CPU.Memory = new Memory(memoryBanks);
            CPU.AfterInstructionExecution += CPU_AfterInstructionExecution;

            var hiaddress = -1;
            foreach (IMemoryBank memoryBank in memoryBanks.OrderBy(m => m.LOW_ADDRESS))
            {
                if (hiaddress+1 < memoryBank.LOW_ADDRESS)
                {
                    CPU.SetMemoryAccessMode((ushort)(hiaddress + 1), memoryBank.LOW_ADDRESS - hiaddress, MemoryAccessMode.NotConnected);
                }
                hiaddress = memoryBank.HI_ADDRESS;
                CPU.SetMemoryAccessMode((ushort)memoryBank.LOW_ADDRESS, memoryBank.SIZE, memoryBank.MemoryAccessMode);
            }

            foreach (var port in ports)
                if (port is IZ80InterruptSource)
                    CPU.RegisterInterruptSource((IZ80InterruptSource)port);

            CPU.MemoryAccess += OnMemoryAccess;
        }

        private void CPU_AfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs e)
        {
            if (_stopRequested)
                e.ExecutionStopper.Stop();
        }

        private void OnMemoryAccess(object sender, MemoryAccessEventArgs e)
        {
            if (e.EventType == MemoryAccessEventType.BeforePortRead 
                || e.EventType == MemoryAccessEventType.BeforePortWrite)
            {
                var port = Ports.FirstOrDefault(p => p.HandledPorts.Contains(e.Address));
                if (port != null)
                {
                    if (e.EventType == MemoryAccessEventType.BeforePortRead)
                        e.Value = port.GetData(e.Address);
                    else
                        port.SetData(e.Address, e.Value);

                    e.CancelMemoryAccess = true;
                }
            }
        }

        public void Start()
        {
            _stopRequested = false;
            CPU.Start();
        }

        public void Resume()
        {
            _stopRequested = false;
            CPU.Continue();
        }

        public void Restart()
        {
            _stopRequested = false;
            ((Memory)CPU.Memory).Reset();
            CPU.Reset();
        }

        public void Stop()
        {
            _stopRequested = true;
        }

    }
}
