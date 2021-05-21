using Konamiman.Z80dotNet;
using Moq;
using NUnit.Framework;
using RC2014.EMU.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Tests
{
    [TestFixture]
    public class MemoryBankTests
    {

        public class MemoryTestBank : MemoryBank
        {
            public override MemoryAccessMode MemoryAccessMode => MemoryAccessMode.ReadAndWrite;

            public byte[] Memory => this.memory;

            public MemoryTestBank(int start, int end)
                : base(start, end)
            {

            }

        }

        [Test]
        public void MemoryIsCorrectSize()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);

            Assert.That(bank.LOW_ADDRESS == 0xFFF0, "Low address set failure");
            Assert.That(bank.HI_ADDRESS == 0xFFFF, "HIGH address set failure");
            Assert.That(bank.SIZE == 16, "Size computed wrong");
            Assert.That(bank.Memory.Length == bank.SIZE, "Memory size isn't correct size");
        }

        [Test]
        public void GetContents1Byte()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);
            for (int i = 0; i < 16; i++)
            {
                bank.Memory[i] = (byte)i;
            }

            for (int i =0;i<16;i++)
            {
                var test = bank.GetContents(0xFFF0 + i, 1);
                Assert.That(test, Has.Length.EqualTo(1));
                Assert.That(test[0] == (byte)i);
            }
        }

        [Test]
        public void GetContentsOverhang()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);
            for (int i = 0; i < 16; i++)
            {
                bank.Memory[i] = (byte)i;
            }

            var test = bank.GetContents(0xFFFF, 2);
            Assert.That(test, Has.Length.EqualTo(1));
            Assert.That(test[0] == 15);
        }


        [Test]
        public void SetContents()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);

            for(int i = 0; i < 16; i++) { 
                bank.SetContents(0xFFF0+i, new byte[] { (byte)(i+1) });
                
                Assert.That(bank.Memory[i] == (byte)(i+1), "{0} bit failed with {1}", i, bank.Memory[i]);
            }
        }

        [Test]
        public void SetContentsOneLoad()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);
            bank.SetContents(0xFFF0, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 0, 16);
            for (int i = 0; i < 16; i++)
            {
                Assert.That(bank.Memory[i] == (byte)(i+1), "{0} failed with {1}", i, bank.Memory[i]);
            }
        }

        [Test]
        public void SetContentsOneLoadOverhang()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);
            bank.SetContents(0xFFF0, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17,18,19, 20 }, 0, 16);
            for (int i = 0; i < 16; i++)
            {
                Assert.That(bank.Memory[i] == (byte)(i+1));
            }

        }

        [Test]
        public void SetContentsOneLoadOverhangMidStream()
        {
            var bank = new MemoryTestBank(0xFFF0, 0xFFFF);
            bank.SetContents(0xFFF0, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 2, 16);
            for (int i = 0; i < 16; i++)
            {
                Assert.That(bank.Memory[i] == (byte)(i+3));
            }
        }

    }
}
