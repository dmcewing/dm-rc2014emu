using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    [Serializable]
    public class RAM64 :MemoryBank
    {
        public override MemoryAccessMode MemoryAccessMode => MemoryAccessMode.ReadAndWrite;

        public RAM64(int start, int end)
           :base(start, end)
        {
        }

        public RAM64()
            :base (0x2000, 0xFFFF)
        {

        }
    }
}
