using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.Module
{
    public class DSRTC : IPort
    {
        protected internal enum States
        {
            NoState = 0,
            CMDLatch = 1,
            DataLatch = 2,
            Burst = 3
        }

        //------ Internal State ------------
        protected internal States State = States.NoState;
        protected internal int commandByte = 0;  //Store for the command byte being acted on.
        protected internal int latches = 0;      //Store for the next incomming byte.
        protected internal int outputLatch = 0;  //Store for the byte being outputted.
        protected internal int latchCnt = 0;     //Counter of bits that have been latched.

        protected internal int lastState = 0;    //Last wire state, so we can track change.

        protected internal readonly byte[] nvram = new byte[31];    //The RAM
        protected internal readonly int[] clkdata = new int[9];     //The RTC Registers.

        protected internal int burst_register = 0;

        //----------------------------------


        protected internal bool Trace { get; set; } = false;
        protected internal bool Trace_Script { get; set; } = false;

        public ushort[] HandledPorts => new ushort[] { 0xC0 };

        public byte GetData(ushort port)
        {
            if (Trace_Script)
                Debug.WriteLine("Debug.WriteLine(\"Read data {{0}}\", n.GetData({0});", port);
            if (Trace)
                Debug.WriteLine("{1}: Reading bit... {0}", (outputLatch & 0x01), State);

            return (byte)((outputLatch & 0x01) & 0xFF);
        }

        public void SetData(ushort port, byte value)
        {
            if (Trace_Script)
                Debug.WriteLine("n.SetData(0x{0:X2}, 0x{1:X2});", port, value);
            /*
            *  RTC_IN	RTC_CLK	RTC_WE	RTC_CE
            *     7       6       5       4    ,   3      2     1      0
            */
            bool CE, WE, Clk, INP;
            (CE, WE, Clk, INP) = GetFlags(value);

            bool chgCE, chgWE, chgClk;
            (chgCE, chgWE, chgClk, _) = GetFlags((byte)((lastState ^ value) & 0xFF)); // & 0xFF - Make sure we only have a byte.

            // If the CE input is low, all data transfer terminates and the I/O pin goes to a high-impedence state.
            if (chgCE)
                if (CE)
                {
                    if (Trace)
                        Debug.WriteLine("{0}: CE Raised", State);

                    State = States.CMDLatch;
                }
                else
                {
                    if (Trace)
                        Debug.WriteLine("{0}: CE Dropped", State);

                    ResetLatches(States.NoState);
                }

            if (chgClk)
            {
                //A clock cycle is a sequence of a rising edge followed bya a falling edge.  For data inputs, data must be valid during the rising
                // edge of the clock and data bits are output on the falling edge of clock.
                if (Clk)
                {
                    //Rising Edge... 
                    latches >>= 1; //first bit in is 0. so ROTR, add at top.
                    latches |= (INP) ? 0x80 : 0x00;
                    latchCnt++;

                    if (Trace)
                        Debug.WriteLine("{2}: Latch now: 0x{0:X2} ({1})", latches, latchCnt, State);

                    if (latchCnt == 8)
                        processLatch();
                }
                else
                {
                    //Falling edge... Cycle output data bits...
                    outputLatch >>= 1;  //rotate output...
                }    
            }

            lastState = value;

        }

        private void processLatch()
        {
            switch (State)
            {
                case States.CMDLatch:
                    commandByte = latches;
                    if (Trace)
                        Debug.WriteLine("{1}: CMD received: 0x{0:X2}", commandByte, State);

                    LockTimeRegisters();
                    if (cmd_Register == 31) //BurstMode enabled
                    {
                        ResetLatches(States.Burst);
                        burst_register = 0;
                        SetOutputRegister(burst_register);
                        if (Trace)
                            Debug.WriteLine("{2}: Burst Mode enabled for {0} ({1})", cmd_IsRam ? "RAM" : "CLOCK", cmd_IsRead ? "R" : "W", State);
                    }
                    else if (!cmd_IsRead)
                    {
                        ResetLatches(States.DataLatch);
                        if (Trace)
                            Debug.WriteLine("{0}: Waiting data byte...", State);
                    }
                    else if (cmd_IsRead)
                    {
                        ResetLatches(States.DataLatch);
                        SetOutputRegister(cmd_Register);
                    }                   
                    break;

                case States.Burst:
                case States.DataLatch:
                    if (Trace)
                        Debug.WriteLine("{1}: Data received: 0x{0:X2}", latches, State);

                    var register = (State == States.Burst) ? burst_register : cmd_Register;

                    if (!cmd_IsRead)
                    {
                        if (cmd_IsRam && !WP)
                        {
                            if (Trace)
                                Debug.WriteLine("{2}: Saving data {1} to RAM reg {0}", register, latches, State);

                            nvram[register % 31] = (byte)(latches & 0xFF);
                        } 
                        else if (cmd_Register == 7 || !WP)
                        {
                            if (Trace)
                                Debug.WriteLine("{2}: Saving data {1} to CLOCK reg {0}", register, latches, State);
                            clkdata[register % 9] = (byte)(latches & 0xFF);
                        }
                    }

                    if (State == States.Burst)
                    {
                        SetOutputRegister(++burst_register); //Ready the register for the next burst....
                    }

                    ResetLatches();
                    
                    break;
            }
        }

        private void SetOutputRegister(int register)
        {
            if (Trace)
                Debug.Write(String.Format("{2}: Set output to {0} reg {1} = ", (cmd_IsRam)?"Ram":"Clock", register, State));

            outputLatch = (cmd_IsRam) ? nvram[register % 31] : clkdata[register % 9];
            outputLatch <<= 1; //this rotates down on cycle.

            if (Trace)
                Debug.WriteLine("0x{0:X2}", outputLatch);

        }

        private void LockTimeRegisters()
        {
            var time = DateTime.Now;

            if (CH || State == States.Burst)
                return;

            clkdata[0] = GetBCD(time.Second); //Seconds;
            clkdata[1] = GetBCD(time.Minute); //Minutes;

            var hour = time.Hour;
            if (!clock24)
            {
                hour %= 12;
                if (hour == 0)
                    hour = 12;
            }
            hour = GetBCD(hour);
            if (!clock24)
            {
                if (time.Hour > 11)
                    hour |= 0x20; //Set AM/PM flag.
                hour |= 0x80;     //Set 12hour flag.
            }
            clkdata[2] = hour; // 12/24, 0, (AM/PM), 10H, H

            clkdata[3] = GetBCD(time.Day);  //Day
            clkdata[4] = GetBCD(time.Month);  //Month
            clkdata[5] = (byte)time.DayOfWeek;

            var year = time.Year % 100;
            clkdata[6] = GetBCD(year);  //10Year, Year
        }

        private static int GetBCD(int value)
        {
            return ((value % 10) + ((value / 10) << 4));
        }

        private void ResetLatches(States newState)
        {
            ResetLatches();
            State = newState;
            if (newState == States.NoState)
            {
                outputLatch = 0;
                burst_register = 0;
            }
        }

        private void ResetLatches()
        {
            latches = 0;
            latchCnt = 0;
        }

        protected internal bool cmd_IsRam => (commandByte & 0x40) != 0;
        protected internal bool cmd_IsRead => (commandByte & 0x01) != 0;
        protected internal int cmd_Register => (commandByte & 0x3E) >> 1;

        protected internal bool CH => (clkdata[0] & 0x80) != 0;
        protected internal bool WP => (clkdata[7] & 0x80) != 0;
        protected internal bool clock24 => (clkdata[2] & 0x80) == 0;

        /// <summary>
        /// Get the flags out of the input byte.
        /// </summary>
        private (bool, bool, bool, bool) GetFlags(byte value)
        {
            bool CE = (value & 0x10) != 0;
            bool WE = (value & 0x20) != 0;
            bool Clk = (value & 0x40) != 0;
            bool INP = (value & 0x80) != 0;

            return (CE, WE, Clk, INP);
        }

        public void SaveState(IFormatter formatter, Stream saveStream)
        {
        }

        public void LoadState(IFormatter formatter, Stream loadStream)
        {
        }
    }
}
