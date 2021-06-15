using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    public abstract class MemoryBank : IMemoryBank
    {
        public int LOW_ADDRESS { get; private set; }

        public int HI_ADDRESS { get; private set; }
        
        public int SIZE { get; private set; }

        public abstract MemoryAccessMode MemoryAccessMode { get; } // => MemoryAccessMode.NotConnected;

        protected readonly byte[] memory;

        public MemoryBank(int start, int end)
        {
            HI_ADDRESS = end;
            LOW_ADDRESS = start;

            SIZE = HI_ADDRESS - LOW_ADDRESS + 1;
            memory = new byte[SIZE];
        }

        public byte[] GetContents(int startAddress, int length)
        {
            Debug.Assert(startAddress >= LOW_ADDRESS && startAddress <= HI_ADDRESS);

            var overhang = startAddress + length -1 - HI_ADDRESS;

            byte[] memSegment;
            if (overhang > 0)
                memSegment = new byte[length - overhang];
            else
                memSegment = new byte[length];

            Array.Copy(memory, startAddress - LOW_ADDRESS, memSegment, 0, memSegment.Length);
            return memSegment;
        }

        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            Debug.Assert(startAddress >= LOW_ADDRESS && startAddress <= HI_ADDRESS);

            if (MemoryAccessMode == MemoryAccessMode.ReadOnly)
                return;

            if (length.HasValue)
            {
                if (HI_ADDRESS - startAddress +1 < length)
                    length -= startAddress + length.Value - HI_ADDRESS;
            }
            else
            {
                length = contents.Length - startIndex;
            }
            
            Array.Copy(contents, startIndex, memory, startAddress - LOW_ADDRESS, length.Value);

        }

        public virtual void Reset() { }
    }
}
