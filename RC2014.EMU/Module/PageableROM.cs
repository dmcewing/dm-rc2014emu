using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    public class PageableROM:MemoryBank
    {
        public override MemoryAccessMode MemoryAccessMode => MemoryAccessMode.ReadOnly;

        public PageableROM(string fileName, short pageSelection = 0, int end = 0x1FFF) : 
            base(0x0, end)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                for(;pageSelection>=0;pageSelection--)
                    fs.Read(memory, 0, SIZE);
            }
        }
    }
}
