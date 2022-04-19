using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zem80.Core;
using Zem80.Core.Memory;
using Z80 = Zem80.Core.Processor;

namespace RC2014.Core
{
    public class RC2014 : IRC2014
    {
        public readonly Z80 CPU;
        public readonly IPort[] Ports; // { get; private set; }
        public readonly IModule[] Modules;
        public readonly IInterruptSource[] InteruptSources;
        
        private bool _stopRequested = false;

        public RC2014(IModule[] modules)
        {
            Modules = modules;

            IMemorySegment[] memorySegments = (from m in modules
                                         where m is IMemorySegment
                                         select m as IMemorySegment).ToArray();

            Ports = (from m in modules
                     where m is IPort
                     select m as IPort).ToArray();

            InteruptSources = (from m in modules
                                    where m is IInterruptSource
                                    select m as IInterruptSource).ToArray();


            // set up the memory map - 16K ROM + 48K RAM = 64K
            IMemoryMap map = new MemoryMap(0x10000, false);
            foreach (var segment in memorySegments)
            {
                map.Map(segment, segment.StartAddress);
            }
            //map.Map(new ReadOnlyMemorySegment(File.ReadAllBytes(romPath)), 0);
            //map.Map(new MemorySegment(49152), 16384);

            CPU = new Z80(map: map, frequencyInMHz: 7.1f);
            CPU.AfterExecute += CPU_AfterExecute;
            CPU.OnClockTick += CPU_OnClockTick;
            //CPU.AfterExecute += DebugOutput_AfterExecute;

            foreach(var isource in InteruptSources)
            {
                isource.InterruptPulse += OnInterruptPulse;
            }

            // We'll connect all ports to the same handlers, which will then work out which device is being addressed and function accordingly.
            for (byte i = 0; i < 255; i++)
            {
                CPU.Ports[i].Connect(ReadPort, WritePort, SignalPortRead, SignalPortWrite);
            }

            //CPU.SetInterruptMode(InterruptMode.IM2);
            CPU.Init(0, false, TimingMode.PseudoRealTime);
            CPU.EnableInterrupts();
        }

        private void OnInterruptPulse(object? sender, EventArgs e)
        {
            CPU.RaiseInterrupt();
        }

        private void CPU_OnClockTick(object? sender, Zem80.Core.Instructions.InstructionPackage e)
        {
            
        }

        private void SignalPortWrite()
        {
//            Debug.WriteLine("Databus (W): " + CPU.IO.DATA_BUS);
        }

        private void SignalPortRead()
        {
 //           Debug.WriteLine("Databus (R): " + CPU.IO.DATA_BUS);
        }

        private void WritePort(byte data)
        {
            ushort portAddress = CPU.IO.ADDRESS_BUS.LowByte();

            var port = Ports.FirstOrDefault(p => p.HandledPorts.Contains(portAddress), null);
            if (port != null)
                port.SetData(portAddress, data);
        }

        private byte ReadPort()
        {
            ushort portAddress = CPU.IO.ADDRESS_BUS.LowByte();
            byte result = 0x0;

            var port = Ports.FirstOrDefault(p => p.HandledPorts.Contains(portAddress), null);
            if (port != null)
            {
                return port.GetData(portAddress);
            }
            return result;
        }

        private void CPU_AfterExecute(object? sender, ExecutionResult e)
        {
            if (_stopRequested)
            {
                CPU.Stop();
                return;
            }
        }

        private Registers _lastRegisters;
        private void DebugOutput_AfterExecute(object? sender, ExecutionResult e)
        {
            string mnemonic = e.Instruction.Mnemonic;
            if (mnemonic.Contains("nn")) mnemonic = mnemonic.Replace("nn", "0x" + e.Data.ArgumentsAsWord.ToString("X4"));
            else if (mnemonic.Contains("n")) mnemonic = mnemonic.Replace("n", "0x" + e.Data.Argument1.ToString("X2"));
            if (mnemonic.Contains("o")) mnemonic = mnemonic.Replace("o", "0x" + e.Data.Argument1.ToString("X2"));
            WriteLog(e.InstructionAddress.ToString("X4") + ": " + mnemonic.PadRight(20));
            regValue(ByteRegister.A); wregValue(WordRegister.BC); wregValue(WordRegister.DE); wregValue(WordRegister.HL); wregValue(WordRegister.SP); wregValue(WordRegister.PC);
            WriteLog(CPU.Flags.State.ToString());

            _lastRegisters = CPU.Registers.Snapshot();
            Console.WriteLine();

            void regValue(ByteRegister r)
            {
                byte value = CPU.Registers[r];
                if (_lastRegisters != null && value != _lastRegisters[r]) Console.ForegroundColor = ConsoleColor.Green;
                WriteLog(r.ToString() + ": 0x" + value.ToString("X2"));
                Console.ForegroundColor = ConsoleColor.White;
                WriteLog(" | ");
            }

            void wregValue(WordRegister r)
            {
                ushort value = CPU.Registers[r];
                if (_lastRegisters != null && value != _lastRegisters[r]) Console.ForegroundColor = ConsoleColor.Green;
                WriteLog(r.ToString() + ": 0x" + value.ToString("X4"));
                Console.ForegroundColor = ConsoleColor.White;
                WriteLog(" | ");
            }
        }

        private void WriteLog(string output)
        {
            Console.Write(output);
        }

        public void Start()
        {
            _stopRequested = false;
            CPU.Start();
        }

        public void Resume()
        {
            _stopRequested = false;
            CPU.Resume();
        }

        public void Restart()
        {
            _stopRequested = true;
            CPU.ResetAndClearMemory();
            CPU.Registers.PC = 0x0;
            Start();
        }

        public void Stop()
        {
            _stopRequested = true;
        }

    }
}
