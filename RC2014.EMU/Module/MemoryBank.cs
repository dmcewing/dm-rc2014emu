using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    [Serializable]
    public abstract class MemoryBank : IMemoryBank
    {
        public int LOW_ADDRESS { get; protected set; }

        public int HI_ADDRESS { get; protected set; }

        public int SIZE { get; protected set; }

        public abstract MemoryAccessMode MemoryAccessMode { get; } // => MemoryAccessMode.NotConnected;

        protected readonly byte[] memory;

        public MemoryBank(int start, int end)
        {
            HI_ADDRESS = end;
            LOW_ADDRESS = start;

            SIZE = HI_ADDRESS - LOW_ADDRESS + 1;
            memory = new byte[SIZE];
        }

        public virtual byte[] GetContents(int startAddress, int length)
        {
            Debug.Assert(startAddress >= LOW_ADDRESS && startAddress <= HI_ADDRESS);

            int overhang = startAddress + length - 1 - HI_ADDRESS;

            byte[] memSegment = overhang > 0
                                ? (new byte[length - overhang])
                                : (new byte[length]);
            Array.Copy(memory, startAddress - LOW_ADDRESS, memSegment, 0, memSegment.Length);
            return memSegment;
        }

        public virtual void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            Debug.Assert(startAddress >= LOW_ADDRESS && startAddress <= HI_ADDRESS);

            if (MemoryAccessMode == MemoryAccessMode.ReadOnly)
            {
                return;
            }

            if (length.HasValue)
            {
                if (HI_ADDRESS - startAddress + 1 < length)
                {
                    length -= startAddress + length.Value - HI_ADDRESS;
                }
            }
            else
            {
                length = contents.Length - startIndex;
            }
            Array.Copy(contents, startIndex, memory, startAddress - LOW_ADDRESS, length.Value);
        }

        public virtual void Reset() { }

        public virtual void SaveState(IFormatter formatter, Stream saveStream)
        {
            formatter.Serialize(saveStream, this);
        }

        public virtual void LoadState(IFormatter formatter, Stream loadStream)
        {
            MemoryBank o = formatter.Deserialize(loadStream) as MemoryBank;
            LOW_ADDRESS = o.LOW_ADDRESS;
            HI_ADDRESS = o.HI_ADDRESS;
            SIZE = o.SIZE;
            for (int i = 0; i < memory.Length; i++)
            {
                memory[i] = o.memory[i];
            }
        }
    }
}
