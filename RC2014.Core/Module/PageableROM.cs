using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Zem80.Core.Memory;

namespace RC2014.Core.Module
{
    public class PageableROM : IMemorySegment, IPort
    {
        private bool isEnabled = true;

        private MemorySegment RAM;
        private IMemorySegment ROM;

        private IMemorySegment ACTIVE_MEMORY => (isEnabled) ? ROM : RAM;

        public ushort[] HandledPorts => new ushort[] { 0x38 };

        public ushort StartAddress => ROM.StartAddress;
        public uint SizeInBytes => ROM.SizeInBytes;

        public bool ReadOnly => isEnabled || (!isEnabled && RAM == null);

        public PageableROM(string fileName, short pageSelection = 0, uint size = 0x2000, bool enableRAM = false, ushort startAddress = 0x0000)
        {
            ROM = new ReadOnlyMemorySegment(LoadRom(fileName, pageSelection, size));

            if (enableRAM)
            {
                RAM = new MemorySegment(ROM.SizeInBytes);
            }

            MapAt(startAddress);
        }

        private static byte[] LoadRom(string fileName, short pageSelection, uint size)
        {
            byte[] page = new byte[size];
            using (FileStream fs = new(fileName, FileMode.Open, FileAccess.Read))
            {
                for (; pageSelection >= 0; pageSelection--)
                {
                    _ = fs.Read(page, 0, (int)size);
                }
            }
            return page;
        }

        public void SaveState(IFormatter formatter, Stream saveStream)
        {
            RAM?.SaveState(formatter, saveStream);
        }

        public void LoadState(IFormatter formatter, Stream loadStream)
        {
            RAM?.LoadState(formatter, loadStream);
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

        public byte ReadByteAt(ushort offset)
        {
            return ACTIVE_MEMORY.ReadByteAt(offset);
        }

        public byte[] ReadBytesAt(ushort offset, int bytes)
        {
            return ACTIVE_MEMORY.ReadBytesAt(offset, bytes);
        }
        public void WriteByteAt(ushort offset, byte value)
        {
            if (ReadOnly)
                return;

            RAM.WriteByteAt(offset, value);
        }

        public void WriteBytesAt(ushort offset, byte[] bytes)
        {
            if (ReadOnly)
                return;

            RAM.WriteBytesAt(offset, bytes);
        }

        public void MapAt(ushort address)
        {
            ROM.MapAt(address);
            RAM?.MapAt(address);
        }

        public void Clear()
        {
            if (RAM != null)
                RAM = new MemorySegment(ROM.SizeInBytes);

            isEnabled = true;
        }
    }
}
