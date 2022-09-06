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

    }
}
