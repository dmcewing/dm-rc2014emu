using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Zem80.Core;
using Zem80.Core.Memory;

namespace RC2014.Core.Module
{
    [Serializable]
    public abstract class MemoryBank : DMMemorySegment, IModule
    {
        public MemoryBank(uint sizeInBytes, ushort startAddress)
            :base(sizeInBytes)
        {
            MapAt(startAddress);
        }

    }

    public class DMMemorySegment : IMemorySegment
    {
        private const uint MAX_SEGMENT_SIZE_IN_BYTES = 65536;
        protected byte[] _memory;

        public ushort StartAddress { get; private set; }

        public uint SizeInBytes { get; private set; }

        public bool ReadOnly => false;

        public byte ReadByteAt(ushort offset)
        {
            return _memory[offset];
        }

        public byte[] ReadBytesAt(ushort offset, int numberOfBytes)
        {
            return _memory[offset..(offset + numberOfBytes)];
        }

        public virtual void WriteByteAt(ushort offset, byte value)
        {
            _memory[offset] = value;
        }

        public virtual void WriteBytesAt(ushort offset, byte[] bytes)
        {
            for (int i = offset; i < offset + bytes.Length; i++)
            {
                _memory[i] = bytes[i - offset];
            }
        }

        public void MapAt(ushort address)
        {
            StartAddress = address;
        }

        public void Clear()
        {
            _memory = new byte[SizeInBytes];
        }

        public void LoadState(IFormatter formatter, Stream loadStream)
        {
            DMMemorySegment o = formatter.Deserialize(loadStream) as DMMemorySegment;
            _memory = o._memory;
        }

        public void SaveState(IFormatter formatter, Stream saveStream)
        {
            formatter.Serialize(saveStream, this);
        }

        public DMMemorySegment(uint sizeInBytes)
        {
            // we have to use uint as the size type because the max permissible size is 65536 bytes which cannot be contained in a ushort - but we now need to do a size check
            if (sizeInBytes > MAX_SEGMENT_SIZE_IN_BYTES) throw new MemorySegmentException("Requested segment size exceeded maximum possible size of " + MAX_SEGMENT_SIZE_IN_BYTES + " bytes.");

            _memory = new byte[sizeInBytes];
            SizeInBytes = sizeInBytes;
        }
    }
}
