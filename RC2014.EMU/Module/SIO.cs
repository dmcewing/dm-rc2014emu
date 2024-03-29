﻿using Konamiman.Z80dotNet;
using RC2014.EMU.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    public class SIO : IPort, IConsoleFeed, IZ80InterruptSource
    {
        [Flags]
        public enum DebugLevel
        {
            None = 0,
            OutputAD = 1,
            OutputBD = 2,
            OutputAC = 4,
            OutputBC = 8,
            OutputA = OutputAC | OutputAD,
            OutputB = OutputBC | OutputBD,
            OutputC = OutputAC | OutputBC,
            OutputD = OutputAD | OutputBD,
            Output = OutputC | OutputD,
            Input = 16,
            Input_Controls = 32,
            All = Output | Input | Input_Controls
        }

        protected internal struct Channel
        {
            public ushort Control;
            public ushort Data;
        }

        private const ushort CHA_Control = 0x80;
        private const ushort CHA_Data = 0x81;
        private const ushort CHB_Control = 0x82;
        private const ushort CHB_Data = 0x83;
        
        private TextWriter Output;
        private readonly Collection<byte> InBytes;

        public event EventHandler NmiInterruptPulse;

        public DebugLevel debugLevel { get; set; } = DebugLevel.None;

        public SIO() => InBytes = new Collection<byte>();

        public void Initalise()
        {
            Console.TreatControlCAsInput = true;
            ConsoleHelper.SetVT100Out();
            Output = Console.Out;
        }

        public void Reset()
        {
            Output = null;
        }

        public ushort[] HandledPorts => new ushort[]
        {
            CHA_Control, CHA_Data, CHB_Control, CHB_Data
        };

        public bool IntLineIsActive => InBytes.Count > 0;

        public byte? ValueOnDataBus => 0;

        public void Write(byte value)
        {
            if ((debugLevel & DebugLevel.Input) > 0)
                Debug.WriteLine("{0:X2} Written to SIO", value);

            InBytes.Add(value);
        }
        public void Write(string value)
        {
            foreach (char v in value.ToCharArray())
            {
                Write(v);
            }
        }
        public void Write(char value)
        {
            Write((byte)value);
        }

        public byte GetData(ushort port)
        {
            if ((debugLevel & DebugLevel.Input_Controls) > 0)
                Debug.WriteLine("Port 0x{0:X2} called for data", port);

            if (port == CHA_Control || port == CHB_Control)
            {
                if (controlLoadSecondByte && writeToRegister != 0) //Return the requested register.
                {
                    byte b = registers[writeToRegister];
                    writeToRegister = 0;
                    controlLoadSecondByte = false;
                    return b;
                }
                else 
                {   //Return RR0
                    controlLoadSecondByte = false;
                    byte b = (InBytes.Count > 0) ? (byte)1 : (byte)0; //RX Char Available.
                    b += (InBytes.Count > 0) ? (byte)2 : (byte)0; //Int Pending
                    b += (byte)4; //Tx Buffer ready
                    return b;
                }
            }
            else if (port == CHA_Data) //Data Port  send first byte from buffer.
            {
                var data = InBytes[0];
                InBytes.RemoveAt(0);
                return data;
            }

            return 0;
        }

        public void SetData(ushort port, byte value)
        {
            DebugWriteLine(port, DebugLevel.Output, "Portd 0x{0:X2} set data to 0x{1:X2}", value);
            if (port == CHA_Data || port == CHB_Data) //CHA_Data
            {
                if (value == 0x0C) //Sending a CLS.
                {
                    Console.Clear();
                }
                else
                    Output?.Write((char)value); //Output the value written.
            }
            else if (port == CHA_Control || port == CHB_Control)
            {
                if (controlLoadSecondByte) //Write the value to the write register.
                {
                    registers[writeToRegister] = value;
                    controlLoadSecondByte = false;
                }
                else  //Set the register to write to.
                {
                    controlLoadSecondByte = true;
                    writeToRegister = value & 0x7; //Only last three bits.
                }
            }
        }

        private void DebugWriteLine(ushort port, DebugLevel level, string message, params object[] args)
        {
            if ((debugLevel & level) == 0)
            {
                return;
            }

            if (
                ((debugLevel & DebugLevel.OutputAD) > 0 && port == CHA_Data)
                || ((debugLevel & DebugLevel.OutputAC) > 0 && port == CHA_Control)
                || ((debugLevel & DebugLevel.OutputBD) > 0 && port == CHB_Data)
                || ((debugLevel & DebugLevel.OutputBC) > 0 && port == CHB_Control)
                )
            {
                List<object> args2 = new List<object>();
                args2.Add(port);
                args2.AddRange(args);
                Debug.WriteLine(message, args2.ToArray());
            }

        }

        public void KeyboardHandler(CancellationToken cancellationToken, KeyPressHandler keyPressHandler)
        {
            do
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo k = Console.ReadKey(true);
                    if (!keyPressHandler(k))
                    {
                        Write(k.KeyChar);
                    }
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        public void SaveState(IFormatter formatter, Stream saveStream)
        {
        }

        public void LoadState(IFormatter formatter, Stream loadStream)
        {
        }

        private bool controlLoadSecondByte = false;
        private int writeToRegister = 0;
        private readonly byte[] registers = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

    }

}
