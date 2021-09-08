using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KTerminalNetworkBDD;
using NUnit.Framework;

namespace KTerminalNetworkBDDTests
{
    [TestFixture]
    class BoundarySetPartitionSetTests
    {
        [Test]
        public void StirlingNumbersAreCorrect()
        {
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(0,0));
            Assert.AreEqual(0, BoundarySetPartitionSet.StirlingNumber(1, 0));
            Assert.AreEqual(0, BoundarySetPartitionSet.StirlingNumber(3, 0));
            Assert.AreEqual(0, BoundarySetPartitionSet.StirlingNumber(8, 0));
            Assert.AreEqual(0, BoundarySetPartitionSet.StirlingNumber(10, 0));
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(1, 1));
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(3, 1));
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(5, 1));
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(10, 1));
            Assert.AreEqual(15, BoundarySetPartitionSet.StirlingNumber(5,2));
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(7, 7));
            Assert.AreEqual(1, BoundarySetPartitionSet.StirlingNumber(9, 9));
            Assert.AreEqual(3025, BoundarySetPartitionSet.StirlingNumber(9, 3));
            Assert.AreEqual(7770, BoundarySetPartitionSet.StirlingNumber(9, 4));
            Assert.AreEqual(42525, BoundarySetPartitionSet.StirlingNumber(10, 5));
            Assert.AreEqual(45, BoundarySetPartitionSet.StirlingNumber(10, 9));
        }

        [Test]
        public void BellNumbersAreCorrect()
        {
            Assert.AreEqual(1, BoundarySetPartitionSet.BellNumber(0));
            Assert.AreEqual(1, BoundarySetPartitionSet.BellNumber(1));
            Assert.AreEqual(2, BoundarySetPartitionSet.BellNumber(2));
            Assert.AreEqual(5, BoundarySetPartitionSet.BellNumber(3));
            Assert.AreEqual(15, BoundarySetPartitionSet.BellNumber(4));
            Assert.AreEqual(52, BoundarySetPartitionSet.BellNumber(5));
            Assert.AreEqual(203, BoundarySetPartitionSet.BellNumber(6));
            Assert.AreEqual(877, BoundarySetPartitionSet.BellNumber(7));
            Assert.AreEqual(4140, BoundarySetPartitionSet.BellNumber(8));
            Assert.AreEqual(21147, BoundarySetPartitionSet.BellNumber(9));
            Assert.AreEqual(115975, BoundarySetPartitionSet.BellNumber(10));
            Assert.AreEqual(678570, BoundarySetPartitionSet.BellNumber(11));
        }

        [Test]
        public void MaxPartitionsIsCorrect()
        {
            Assert.AreEqual(22, BoundarySetPartitionSet.MaxPartitions(3, 3));
        }

        [Test]
        public void BoundarySetPartitionSetValidityCheckIsCorrect()
        {
            byte[] partitionsA = new byte[] {0, 3, 1, 3};
            bool[] partitionMarkingsA = new bool[] {true, false, true, false};
            var bsps = new BoundarySetPartitionSet(partitionsA, partitionMarkingsA);
            Assert.IsFalse(bsps.IsValid());

            byte[] partitionsB = new byte[]{2,1,0};
            bool[] partitionMarkingsB = new bool[]{true, false, true};
            bsps = new BoundarySetPartitionSet(partitionsB, partitionMarkingsB);
            Assert.IsFalse(bsps.IsValid());

            byte[] partitionsC = new byte[] { 0, 3, 1, 3 };
            bool[] partitionMarkingsC = new bool[] { true, false, false, false };
            bsps = new BoundarySetPartitionSet(partitionsC, partitionMarkingsC);
            Assert.IsFalse(bsps.IsValid());

        }
    }
}
