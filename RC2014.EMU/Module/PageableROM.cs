using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.EMU.Module
{
    public class PageableROM : MemoryBank, IPort
    {
        private bool isEnabled = true;
        public override MemoryAccessMode MemoryAccessMode => (RAM == null)?MemoryAccessMode.ReadOnly:MemoryAccessMode.ReadAndWrite;

        private readonly IMemoryBank RAM;

        public ushort[] HandledPorts => new ushort[] { 0x38 };

        public PageableROM(string fileName, short pageSelection = 0, int end = 0x1FFF, IMemoryBank ram = null) :
            base(0x0, ram?.HI_ADDRESS??end)
        {
            LoadRom(fileName, pageSelection, end);
            RAM = ram;
        }

        public override byte[] GetContents(int startAddress, int length)
        {
            if (isEnabled)
                return base.GetContents(startAddress, length);
            else if (RAM != null)
                return RAM.GetContents(startAddress, length);

            return new byte[] { 0x0 };
        }

        public override void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            if (isEnabled && startAddress < 0xFFFF)
                base.SetContents(startAddress, contents, startIndex, length);
            else if (RAM != null)
                RAM.SetContents(startAddress, contents, startIndex, length);
        }

        private void LoadRom(string fileName, short pageSelection, int end)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                for (; pageSelection >= 0; pageSelection--)
                {
                    _ = fs.Read(memory, 0, end);
                }
            }
        }

        public override void SaveState(IFormatter formatter, Stream saveStream)
        {
            RAM?.SaveState(formatter, saveStream);
        }

        public override void LoadState(IFormatter formatter, Stream loadStream)
        {
            RAM?.LoadState(formatter, loadStream);
        }

        public override void Reset()
        {
            base.Reset();
            isEnabled = true;
        }

        public byte GetData(ushort port)
        {
            return 0;
        }

        public void SetData(ushort port, byte value)
        {
            System.Diagnostics.Debug.WriteLine($"Pageable {port:X2}, {value:X2}");
            isEnabled = !isEnabled;
        }
    }
}
