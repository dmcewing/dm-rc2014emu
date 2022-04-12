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
        public readonly IModule[] Modules;

        private bool _stopRequested = false;

        public RC2014(IZ80Processor cpu, IModule[] modules)
        {
            Modules = modules;

            //IPort[] ports = modules.Where(m => m is IPort).Cast<IPort>().ToArray();
            IPort[] ports = (from m in modules
                             where m is IPort
                             select m as IPort).ToArray();

            //IMemoryBank[] memoryBanks = modules.Where(m => m is IMemoryBank).Cast<IMemoryBank>().ToArray();
            IMemoryBank[] memoryBanks = (from m in modules
                                         where m is IMemoryBank
                                         select m as IMemoryBank).ToArray();

            CPU = cpu;
            Ports = ports;
            CPU.Registers = new NotifiedZ80Registers();
            CPU.Memory = new Memory(memoryBanks);
            CPU.AfterInstructionExecution += CPU_AfterInstructionExecution;

            int hiaddress = -1;
            foreach (IMemoryBank memoryBank in memoryBanks.OrderBy(m => m.LOW_ADDRESS))
            {
                if (hiaddress + 1 < memoryBank.LOW_ADDRESS)  //Gap in memory.  Set the gap range as NotConnected.
                {
                    CPU.SetMemoryAccessMode((ushort)(hiaddress + 1), memoryBank.LOW_ADDRESS - hiaddress, MemoryAccessMode.NotConnected);
                }

                hiaddress = memoryBank.HI_ADDRESS;
                CPU.SetMemoryAccessMode((ushort)memoryBank.LOW_ADDRESS, memoryBank.SIZE, memoryBank.MemoryAccessMode);
            }

            foreach (var source in from IPort port in ports
                                   where port is IZ80InterruptSource
                                   select port as IZ80InterruptSource)
            {
                CPU.RegisterInterruptSource(source);
            }

            CPU.MemoryAccess += OnMemoryAccess;
        }

        private void CPU_AfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs e)
        {
            if (_stopRequested)
            {
                e.ExecutionStopper.Stop();
            }
        }

        private void OnMemoryAccess(object sender, MemoryAccessEventArgs e)
        {
            if (e.EventType == MemoryAccessEventType.BeforePortRead
                || e.EventType == MemoryAccessEventType.BeforePortWrite)
            {
                IPort port = Ports.FirstOrDefault(p => p.HandledPorts.Contains(e.Address));
                if (port != null)
                {
                    if (e.EventType == MemoryAccessEventType.BeforePortRead)
                    {
                        e.Value = port.GetData(e.Address);
                    }
                    else
                    {
                        port.SetData(e.Address, e.Value);
                    }

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
            _stopRequested = true;
            (CPU.Memory as Memory)?.Reset();
            CPU.Reset();
            CPU.Registers.PC = 0x0;
            Start();
        }

        public void Stop()
        {
            _stopRequested = true;
        }

    }
}
