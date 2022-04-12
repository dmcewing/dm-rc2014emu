using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    [Serializable]
    public class RAM32 : MemoryBank
    {
        public override MemoryAccessMode MemoryAccessMode => MemoryAccessMode.ReadAndWrite;

        public RAM32()
            :base(0x8000, 0xFFFF)
        {
        }

    }
}
