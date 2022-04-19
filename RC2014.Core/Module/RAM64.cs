using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.Module
{
    [Serializable]
    public class RAM64 :MemoryBank
    {
        public RAM64(ushort start)
           :base(0x8000, start)
        {
        }

        public RAM64()
            :base (0x8000, 0x2000)
        {
        }
    }
}
