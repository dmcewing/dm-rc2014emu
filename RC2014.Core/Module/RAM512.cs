using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Zem80.Core.Memory;

namespace RC2014.Core.Module
{
    /// <summary>
    /// Zeta SBC V2 features a 512 KiB Flash ROM, and 512 KiB SRAM. The Flash ROM is used for the boot loader, monitor, 
    ///    OS(CP/M and ZSDOS at this point), and ROM disk.The SRAM is battery-backed, and is used for applications and for a RAM disk.
    /// </summary>
    /// <remarks>
    /// Following a power on or a hard reset the memory paging mechanism is disabled, 
    /// and memory address lines MA19 - MA14 are pulled down.
    /// 
    /// So that page #0 (ROM addresses 00000h to 03FFF) is mapped to all four banks. 
    /// 
    /// That page should contain a boot loader that (among other things) configures and enables memory paging.
    /// 
    /// Once page select registers are configured properly the memory paging can be enabled by setting bit 1 of MPGENA(7Ch) register.
    /// </remarks>
    [Serializable]
    public class RAM512 : IMemorySegment, IPort
    {
        public const int k16 = 0x4000;  //Constant to represent 16KiB.

        // Name the port addresses.
        public const ushort MPGSEL_0 = 0x78;
        public const ushort MPGSEL_1 = 0x79;
        public const ushort MPGSEL_2 = 0x7A;
        public const ushort MPGSEL_3 = 0x7B;
        public const ushort MPGENA = 0x7C;

        /// <summary>
        /// Page select registers are used to map physical memory pages to the banks in Z80 address space:
        ///     MPGSEL_0(78h) - Page select register for bank #0
        ///     MPGSEL_1(79h) - Page select register for bank #1
        ///     MPGSEL_2(7Ah) - Page select register for bank #2
        ///     MPGSEL_3(7Bh) - Page select register for bank #3
        /// </summary>
        public ushort[] HandledPorts => new ushort[]
            {
                MPGSEL_0,
                MPGSEL_1,
                MPGSEL_2,
                MPGSEL_3,
                MPGENA
            };

        public bool debugOn { get; set; } = false;

        //Internal structure to manage the settings for a BANK of memory.
        [Serializable]
        internal class BankSetting
        {
            public BankSetting(byte page, int low, int high)
            {
                SelectedPage = page;
                LOW = low;
                HIGH = high;
            }
            public byte SelectedPage { get; set; }
            public int LOW { get; set; }
            public int HIGH { get; set; }
        }

        /// <summary>
        /// The physical memory(512 KiB Flash ROM and 512 KiB SRAM) is divided into 16 KiB pages:
        /// Pages 0 - 31 are mapped to the Flash ROM: page #0 starts at ROM address 00000h, and page #31 ends at ROM address 7FFFFh.
        /// Pages 32 - 63 are mapped to the SRAM: page #32 starts at RAM address 00000h, and page #63 ends at RAM address 7FFFFh.
        /// </summary>
        protected internal byte[][] memory;

        /// <summary>
        /// Memory Banks and Paging
        /// The 64 KiB Z80 memory address space is divided into four 16 KiB memory banks:
        ///     Bank #0 (0000h - 3FFFh
        ///     Bank #1 (4000h - 7FFFh)
        ///     Bank #2 (8000h - 0BFFFh)
        ///     Bank #3 (0C000h - 0FFFFh)
        /// </summary>
        internal BankSetting[] bankSettings = new BankSetting[4] { 
            new BankSetting(0, 0x0000, 0x3FFF),
            new BankSetting(0, 0x4000, 0x7FFF),
            new BankSetting(0, 0x8000, 0xBFFF),
            new BankSetting(0, 0xC000, 0xFFFF)
        };
        private bool pageEnable = false;

        public RAM512(string fileName)
        {
            Clear();
            
            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            for (int i = 0; i <= 31; i++)
            {
                _ = fs.Read(memory[i], 0, k16);
            }
        }

        public RAM512()
        {
            Clear();
        }
              
        public byte GetData(ushort port)
        {
            switch (port)
            {
                case MPGENA:
                    return pageEnable ? (byte)1 : (byte)0;

                case MPGSEL_0:
                    return bankSettings[0].SelectedPage;

                case MPGSEL_1:
                    return bankSettings[1].SelectedPage; 

                case MPGSEL_2:
                    return bankSettings[2].SelectedPage; 

                case MPGSEL_3:
                    return bankSettings[3].SelectedPage;
            }

            return 0;
        }

        public void SetData(ushort port, byte value)
        {
            if (debugOn)
                Debug.WriteLine("Port 0x{0:X2} called with {1}", port,value);
            switch (port)
            {
                case MPGENA:
                    pageEnable = (value & 1) > 0;
                    break;

                case MPGSEL_0:
                    bankSettings[0].SelectedPage = value;
                    break;

                case MPGSEL_1:
                    bankSettings[1].SelectedPage = value;
                    break;

                case MPGSEL_2:
                    bankSettings[2].SelectedPage = value;
                    break;

                case MPGSEL_3:
                    bankSettings[3].SelectedPage = value;
                    break;
            }
        }

        public byte[] GetContents(int startAddress, int length)
        {
            byte[] memorySegment;

            BankSetting? bank = bankSettings.FirstOrDefault(m => m.LOW <= startAddress && startAddress <= m.HIGH);
            if (bank == null)
                return new byte[1] { 0 };
            
            byte[] segment1 = GetContents(bank, startAddress, length);

            if (startAddress + length - 1 > bank.HIGH)
            {
                memorySegment = new byte[length];
                Array.Copy(segment1, 0, memorySegment, 0, segment1.Length);

                byte[] segment2 = GetContents(bank.HIGH + 1, length - segment1.Length);
                Array.Copy(segment2, 0, memorySegment, segment1.Length, segment2.Length);
            }
            else
            {
                memorySegment = segment1;
            }

            return memorySegment;
        }

        private byte[] GetContents(BankSetting bank, int startAddress, int length)
        {
            int overhang = startAddress + length - 1 - bank.HIGH;

            byte[] memSegment = overhang > 0
                                    ? (new byte[length - overhang])
                                    : (new byte[length]);

            Array.Copy(memory[bank.SelectedPage], startAddress - bank.LOW, memSegment, 0, memSegment.Length);
            return memSegment;
        }

        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            if (debugOn)
                Debug.Assert(startAddress >= 0x0000 && startAddress <= 0xFFFF);

            if (!length.HasValue)
            {
                length = contents.Length - startIndex;
            }

            BankSetting bank = bankSettings.FirstOrDefault(m => m.LOW <= startAddress && startAddress <= m.HIGH);
            if (bank == null)
                return;

            SetContents(bank, startAddress, contents, startIndex, length);

            int overflow = (startAddress + length.Value) - bank.HIGH - 1;
            if (overflow > 0)
            {
                int spaceInBank = bank.HIGH - startAddress + 1;
                SetContents(bank.HIGH + 1, contents, startIndex + spaceInBank, overflow);
            }
        }

        private void SetContents(BankSetting bank, int startAddress, byte[] contents, int startIndex, int? length)
        {
            if (bank.SelectedPage <= 31)
            {
                return;
            }

            if (length.HasValue)
            {
                if (bank.HIGH - startAddress + 1 < length)
                {
                    length -= startAddress + length.Value - bank.HIGH;
                }
            }
            else
            {
                length = contents.Length - startIndex;
            }

            Array.Copy(contents, startIndex, memory[bank.SelectedPage], startAddress - bank.LOW, length.Value);
        }

        public void SaveState(IFormatter formatter, Stream saveStream)
        {
            formatter.Serialize(saveStream, this);
        }

        public void LoadState(IFormatter formatter, Stream loadStream)
        {
            RAM512 o = formatter.Deserialize(loadStream) as RAM512;
            memory = o.memory;
            bankSettings = o.bankSettings;
            pageEnable = o.pageEnable;
        }

        public byte ReadByteAt(ushort offset)
        {
            return GetContents(offset, 1)[0];
        }

        public byte[] ReadBytesAt(ushort offset, int bytes)
        {
            return GetContents(offset, bytes);
        }

        public void WriteByteAt(ushort offset, byte value)
        {
            SetContents(offset, new byte[] { value });
        }

        public void WriteBytesAt(ushort offset, byte[] bytes)
        {
            SetContents(offset, bytes);
        }

        public void MapAt(ushort address)
        {
            if (address != 0x000)
                throw new ApplicationException("Cannot map 512k RAM Module at a location other than 0x0000");
        }

        public void Clear()
        {
            memory = new byte[][]
            {
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16], new byte[k16],
                new byte[k16], new byte[k16], new byte[k16], new byte[k16]
            };

            for (int i = 0; i < 4; i++)
            {
                bankSettings[i].SelectedPage = 0;
            }
        }

        public ushort StartAddress => 0x0000;

        public uint SizeInBytes => 0x10000;

        public bool ReadOnly => false;
    }
}
