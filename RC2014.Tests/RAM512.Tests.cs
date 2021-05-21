using Moq;
using NUnit.Framework;
using RC2014.EMU;
using RC2014.EMU.Module;
using System;


namespace RC2014.Tests
{
    [TestFixture]
    public class RAM512Tests
    {
        [Test]
        public void ThatRamIsDevidedIntoFourBanks()
        {
            var ram = new RAM512();
            Assert.That(ram.bankSettings, Has.Length.EqualTo(4));
        }

        [Test]
        public void ThatRAMPageSettingWorks()
        {
            var ram = new RAM512();
            ram.SetData(ram.HandledPorts[0], 1);
            ram.SetData(ram.HandledPorts[1], 2);
            ram.SetData(ram.HandledPorts[2], 3);
            ram.SetData(ram.HandledPorts[3], 4);

            Assert.AreEqual(1, ram.bankSettings[0].SelectedPage);
            Assert.AreEqual(2, ram.bankSettings[1].SelectedPage);
            Assert.AreEqual(3, ram.bankSettings[2].SelectedPage);
            Assert.AreEqual(4, ram.bankSettings[3].SelectedPage);
        }

        [Test]
        public void ThatRAMPageGettingWorks()
        {
            var ram = new RAM512();
            ram.bankSettings[0].SelectedPage = 10;
            ram.bankSettings[1].SelectedPage = 11;
            ram.bankSettings[2].SelectedPage = 12;
            ram.bankSettings[3].SelectedPage = 13;

            Assert.AreEqual(10, ram.GetData(ram.HandledPorts[0]));
            Assert.AreEqual(11, ram.GetData(ram.HandledPorts[1]));
            Assert.AreEqual(12, ram.GetData(ram.HandledPorts[2]));
            Assert.AreEqual(13, ram.GetData(ram.HandledPorts[3]));
        }

        [Test]
        public void That_Writing_a_Ram_bank_writes_bank0()
        {
            var ram = new RAM512();
            ram.bankSettings[0].SelectedPage = 32;
            ram.SetContents(0x0000, new byte[] { 1, 2, 3, 4 }, 0, 4);

            Assert.AreEqual(1, ram.memory[32][0]);
            Assert.AreEqual(2, ram.memory[32][1]);
            Assert.AreEqual(3, ram.memory[32][2]);
            Assert.AreEqual(4, ram.memory[32][3]);
        }
        [Test]
        public void That_Writing_a_Ram_bank_writes_bank1()
        {
            var ram = new RAM512();
            ram.bankSettings[1].SelectedPage = 32;
            ram.SetContents(0x4000, new byte[] { 1, 2, 3, 4 }, 0, 4);

            Assert.AreEqual(1, ram.memory[32][0]);
            Assert.AreEqual(2, ram.memory[32][1]);
            Assert.AreEqual(3, ram.memory[32][2]);
            Assert.AreEqual(4, ram.memory[32][3]);
        }
        [Test]
        public void That_Writing_a_Ram_bank_writes_bank2()
        {
            var ram = new RAM512();
            ram.bankSettings[2].SelectedPage = 32;
            ram.SetContents(0x8000, new byte[] { 1, 2, 3, 4 }, 0, 4);

            Assert.AreEqual(1, ram.memory[32][0]);
            Assert.AreEqual(2, ram.memory[32][1]);
            Assert.AreEqual(3, ram.memory[32][2]);
            Assert.AreEqual(4, ram.memory[32][3]);
        }
        [Test]
        public void That_Writing_a_Ram_bank_writes_bank3()
        {
            var ram = new RAM512();
            ram.bankSettings[3].SelectedPage = 32; 
            ram.SetContents(0xC000, new byte[] { 1, 2, 3, 4 }, 0, 4);

            Assert.AreEqual(1, ram.memory[32][0]);
            Assert.AreEqual(2, ram.memory[32][1]);
            Assert.AreEqual(3, ram.memory[32][2]);
            Assert.AreEqual(4, ram.memory[32][3]);
        }

        [Test]
        public void That_Reading_ContentsWorks()
        {
            var ram = new RAM512();
            ram.memory[0][1] = 24;
            ram.memory[0][10] = 24;
            ram.memory[0][20] = 24;

            var b0 = ram.GetContents(0x0000, 21);
            var b1 = ram.GetContents(0x4000, 21);
            var b2 = ram.GetContents(0x8000, 21);
            var b3 = ram.GetContents(0xC000, 21);

            Assert.That(b1, Has.Length.EqualTo(21));
            Assert.AreEqual(b0, b1);
            Assert.AreEqual(b0, b2);
            Assert.AreEqual(b0, b3);
            Assert.AreEqual(24, b0[1]);
            Assert.AreEqual(24, b0[10]);
            Assert.AreEqual(24, b0[20]);
        }

        [Test]
        public void That_Reading_Accross_Page_Boundary_works()
        {
            Assert.Inconclusive("Need to code this test.");
        }
    }
}
