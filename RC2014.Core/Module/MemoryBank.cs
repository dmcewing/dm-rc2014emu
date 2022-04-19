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
    [Serializable]
    public abstract class MemoryBank : MemorySegment, IModule
    {
        public MemoryBank(uint sizeInBytes, ushort startAddress)
            :base(sizeInBytes)
        {
            MapAt(startAddress);
        }

        public virtual void SaveState(IFormatter formatter, Stream saveStream)
        {
            //formatter.Serialize(saveStream, this);
        }

        public virtual void LoadState(IFormatter formatter, Stream loadStream)
        {
            //MemoryBank o = formatter.Deserialize(loadStream) as MemoryBank;
            //StartAddress = o.StartAddress;
            //SizeInBytes = o.SizeInBytes;
            //for (int i = 0; i < memory.Length; i++)
            //{
            //    memory[i] = o.memory[i];
            //}
        }
    }
}
