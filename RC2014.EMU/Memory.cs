using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU
{
    internal class Memory : IMemory, INotifyPropertyChanged
    {
        private readonly IMemoryBank[] MemoryBanks;
        private readonly int MAX_MEMORY;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public Memory(IMemoryBank[] memoryBanks)
        {
            MemoryBanks = memoryBanks;
            MAX_MEMORY = memoryBanks.Max(m => m.HI_ADDRESS); //Assumes all banks are loaded.
        }
        public void Reset()
        {
            foreach (IMemoryBank bank in MemoryBanks)
            {
                bank.Reset();
            }
        }

        public byte this[int address]
        {
            get => GetContents(address, 1)[0];
            set
            {
                SetContents(address, new byte[] { value });
                NotifyPropertyChanged();
            }
        }

        public int Size => MAX_MEMORY + 1;

        public byte[] GetContents(int startAddress, int length)
        {
            byte[] memorySegment;

            IMemoryBank bank = MemoryBanks.FirstOrDefault(m => m.LOW_ADDRESS <= startAddress && startAddress <= m.HI_ADDRESS);
            if (bank == null)
            {
                return new byte[1] { 0 };
            }

            byte[] segment1 = bank?.GetContents(startAddress, length);

            if (startAddress + length - 1 > bank.HI_ADDRESS)
            {
                memorySegment = new byte[length];
                Array.Copy(segment1, 0, memorySegment, 0, segment1.Length);

                byte[] segment2 = GetContents(bank.HI_ADDRESS + 1, length - segment1.Length);
                Array.Copy(segment2, 0, memorySegment, segment1.Length, segment2.Length);
            }
            else
            {
                memorySegment = segment1;
            }

            return memorySegment;
        }

        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            if (!length.HasValue)
            {
                length = contents.Length - startIndex;
            }

            IMemoryBank bank = MemoryBanks.FirstOrDefault(m => m.LOW_ADDRESS <= startAddress && startAddress <= m.HI_ADDRESS);
            if (bank == null)
            {
                return;
            }

            bank.SetContents(startAddress, contents, startIndex, length);

            int overflow = startAddress + length.Value - bank.HI_ADDRESS - 1;
            if (overflow > 0)
            {
                int spaceInBank = bank.HI_ADDRESS - startAddress + 1;
                SetContents(bank.HI_ADDRESS + 1, contents, startIndex + spaceInBank, overflow);
            }
        }
    }
}
