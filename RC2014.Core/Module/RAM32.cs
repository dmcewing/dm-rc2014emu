using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Zem80.Core.Memory;

namespace RC2014.Core.Module
{
    [Serializable]
    public class RAM32 : MemoryBank
    {
        public RAM32()
            :base(0x4000, 0x8000)
        {
        }
        public RAM32(ushort start)
            :base(0x4000, start)
        {

        }
    }
}
