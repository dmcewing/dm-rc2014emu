using Moq;
using NUnit.Framework;
using RC2014.EMU;
using System;

namespace RC2014.Tests
{
    [TestFixture]
    public class MemoryTests
    {
        [Test]
        public void Constructs()
        {
            var mb = new Mock<IMemoryBank>();
            mb.SetupGet(b => b.HI_ADDRESS).Returns(0xFFFF);
            
            IMemoryBank[] mbs = new IMemoryBank[]
            {
                mb.Object
            };

            var memory = new RC2014.EMU.Memory(mbs);

            mb.VerifyAll();
            Assert.AreEqual(0x10000, memory.Size);

        }

        [Test]
        public void GetMemoryCrossPage4Bytes()
        {
            var mb = new Mock<IMemoryBank>();
            mb.SetupGet(b => b.HI_ADDRESS).Returns(0xFFFF);
            mb.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFF0);
            var mb2 = new Mock<IMemoryBank>();
            mb2.SetupGet(b => b.HI_ADDRESS).Returns(0xFFEF);
            mb2.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFE0);

            mb.Setup(b => b.GetContents(0xFFF0, 2)).Returns(new byte[] { 3, 4 }).Verifiable("Didn't get two from second bank");
            mb2.Setup(b => b.GetContents(0xFFEE, 4)).Returns(new byte[] { 1, 2 }).Verifiable("Didn't get four from first bank");

            IMemoryBank[] mbs = new IMemoryBank[]
            {
                mb.Object, mb2.Object
            };

            var memory = new RC2014.EMU.Memory(mbs);

            var bytes = memory.GetContents(0xFFEE, 4);

            mb.VerifyAll();
            mb2.VerifyAll();

            Assert.That(bytes, Has.Length.EqualTo(4));
            Assert.AreEqual(new byte[] { 1, 2, 3, 4 }, bytes);
        }

        [Test]
        public void GetMemoryCrossPage2Bytes()
        {
            var mb = new Mock<IMemoryBank>();
            mb.SetupGet(b => b.HI_ADDRESS).Returns(0xFFFF);
            mb.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFF0);
            var mb2 = new Mock<IMemoryBank>();
            mb2.SetupGet(b => b.HI_ADDRESS).Returns(0xFFEF);
            mb2.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFE0);

            mb.Setup(b => b.GetContents(0xFFF0, 1)).Returns(new byte[] { 3 }).Verifiable("Didn't get two from second bank");
            mb2.Setup(b => b.GetContents(0xFFEF, 2)).Returns(new byte[] { 2 }).Verifiable("Didn't get four from first bank");

            IMemoryBank[] mbs = new IMemoryBank[]
            {
                mb.Object, mb2.Object
            };

            var memory = new RC2014.EMU.Memory(mbs);

            var bytes = memory.GetContents(0xFFEF, 2);

            mb.VerifyAll();
            mb2.VerifyAll();

            Assert.That(bytes, Has.Length.EqualTo(2));
            Assert.AreEqual(new byte[] { 2, 3 }, bytes);
        }

        [Test]
        public void GetMemoryTopMostByte()
        {
            var mb = new Mock<IMemoryBank>();
            mb.SetupGet(b => b.HI_ADDRESS).Returns(0xFFFF);
            mb.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFF0);

            mb.Setup(b => b.GetContents(0xFFFF, 1)).Returns(new byte[] { 9 }).Verifiable("Didn't get two from second bank");
            
            IMemoryBank[] mbs = new IMemoryBank[]
            {
                mb.Object
            };

            var memory = new RC2014.EMU.Memory(mbs);

            var bytes = memory.GetContents(0xFFFF, 1);

            mb.VerifyAll();

            Assert.That(bytes, Has.Length.EqualTo(1));
            Assert.AreEqual(new byte[] { 9 }, bytes);
        }

        [Test]
        public void GetMemoryTopBottomByte()
        {
            var mb = new Mock<IMemoryBank>();
            mb.SetupGet(b => b.HI_ADDRESS).Returns(0xFFFF);
            mb.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFF0);

            mb.Setup(b => b.GetContents(0xFFF0, 1)).Returns(new byte[] { 9 }).Verifiable("Didn't get two from second bank");

            IMemoryBank[] mbs = new IMemoryBank[]
            {
                mb.Object
            };

            var memory = new RC2014.EMU.Memory(mbs);

            var bytes = memory.GetContents(0xFFF0, 1);

            mb.VerifyAll();

            Assert.That(bytes, Has.Length.EqualTo(1));
            Assert.AreEqual(new byte[] { 9 }, bytes);
        }

        [Test]
        public void SetMemoryCrossPages()
        {
            var mb = new Mock<IMemoryBank>();
            mb.SetupGet(b => b.HI_ADDRESS).Returns(0xFFFF);
            mb.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFF0);
            var mb2 = new Mock<IMemoryBank>();
            mb2.SetupGet(b => b.HI_ADDRESS).Returns(0xFFEF);
            mb2.SetupGet(b => b.LOW_ADDRESS).Returns(0xFFE0);

            byte[] newMem = new byte[] { 1, 2, 3, 4 };

            mb.Setup(b => b.SetContents(0xFFF0, newMem, 2, 2)).Verifiable("Didn't set the second bank");
            mb2.Setup(b => b.SetContents(0xFFEE, newMem, 0, 4)).Verifiable("Didn't set four on the first bank");

            IMemoryBank[] mbs = new IMemoryBank[]
            {
                mb.Object, mb2.Object
            };

            var memory = new RC2014.EMU.Memory(mbs);
            memory.SetContents(0xFFEE, new byte[] { 1, 2, 3, 4 }, 0, 4);

            mb2.VerifyAll();
            mb.VerifyAll();
            mb.Verify(x => x.SetContents(It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Once);
            mb2.Verify(x => x.SetContents(It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Once);

        }
    }
}
