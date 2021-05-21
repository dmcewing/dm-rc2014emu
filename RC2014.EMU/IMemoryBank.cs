using Konamiman.Z80dotNet;

namespace RC2014.EMU
{
    public interface IMemoryBank: IModule
    {
        int LOW_ADDRESS { get; }
        int HI_ADDRESS { get; }
        int SIZE { get; }

        MemoryAccessMode MemoryAccessMode { get; }

        byte[] GetContents(int startAddress, int length);

        void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null);

        void Reset();
    }
}