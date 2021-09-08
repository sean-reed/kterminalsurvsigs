using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using KTerminalNetworkBDD;

namespace KTerminalNetworkBDDTests
{
    [TestFixture]
    class NDArrayTests
    {
        NDArray a, b, c, d, e;

        [SetUp]
        public void Setup()
        {
            a = NDArray.FromValues(new double[] { 5, 4, 8, 8 });

            b = NDArray.FromValues(new double[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } });

            c = NDArray.FromValues(new double[,,] { { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }, { { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 } } });

            d = NDArray.FromValues(new double[,,] { { { 4, 7, 3 }, { 1, 1, 1 }, { 7, 4, 4 } }, { { 1, 2, 3 }, { 5, 5, 7 }, { 3, 2, 2 } } });

            e = NDArray.FromValues(new double[,,] { { { 5, 9, 6 }, { 5, 6, 7 }, { 14, 12, 13 } }, { { 11, 13, 15 }, { 18, 19, 22 }, { 19, 19, 20 } } });
        }

        [Test]
        public void ShapesAreCorrect()
        {
            Assert.That(a.Shape, Is.EqualTo(new int[] { 4 }));
            Assert.That(b.Shape, Is.EqualTo(new int[] { 2, 4 }));
            Assert.That(c.Shape, Is.EqualTo(new int[] { 2, 3, 3 }));
        }

        [Test]
        public void StridesAreCorrect()
        {
            Assert.That(a.Strides, Is.EqualTo(new int[] { 1 }));
            Assert.That(b.Strides, Is.EqualTo(new int[] { 4, 1 }));
            Assert.That(c.Strides, Is.EqualTo(new int[] { 9, 3, 1 }));
        }

        [Test]
        public void InitialValuesAreCorrect()
        {
            Assert.That(a.GetValue(new int[] { 0 }), Is.EqualTo(5));
            Assert.That(a.GetValue(new int[] { 1 }), Is.EqualTo(4));
            Assert.That(a.GetValue(new int[] { 2 }), Is.EqualTo(8));
            Assert.That(a.GetValue(new int[] { 3 }), Is.EqualTo(8));

            Assert.That(b.GetValue(new int[] { 0, 0 }), Is.EqualTo(1));
            Assert.That(b.GetValue(new int[] { 1, 1 }), Is.EqualTo(6));
            Assert.That(b.GetValue(new int[] { 0, 3 }), Is.EqualTo(4));
            Assert.That(b.GetValue(new int[] { 1, 2 }), Is.EqualTo(7));

            Assert.That(c.GetValue(new int[] { 0, 0, 0 }), Is.EqualTo(1));
            Assert.That(c.GetValue(new int[] { 0, 2, 0 }), Is.EqualTo(7));
            Assert.That(c.GetValue(new int[] { 1, 1, 1 }), Is.EqualTo(14));
            Assert.That(c.GetValue(new int[] { 1, 2, 2 }), Is.EqualTo(18));
            Assert.That(c.GetValue(new int[] { 1, 2, 1 }), Is.EqualTo(17));
            Assert.That(c.GetValue(new int[] { 1, 1, 2 }), Is.EqualTo(15));
        }

        [Test]
        public void ArraySumOperationIsCorrect()
        {
            Assert.True(NDArray.ArrayEqual(NDArray.Sum(c, d), e));
        }

        [Test]
        public void ShiftOneOperationIsCorrect()
        {
            NDArray expected = NDArray.FromValues(new double[] { 0, 5, 4, 8 });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(a, 0), expected));

            expected = NDArray.FromValues(new double[] { 0, 0, 5, 4 });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(NDArray.GetOneShifted(a, 0), 0), expected));

            expected = NDArray.FromValues(new double[,] { { 0, 0, 0, 0 }, { 1, 2, 3, 4 } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(b, 0), expected));
            expected = NDArray.FromValues(new double[,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(NDArray.GetOneShifted(b, 0), 0), expected));

            expected = NDArray.FromValues(new double[,] { { 0, 1, 2, 3 }, { 0, 5, 6, 7 } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(b, 1), expected));
            expected = NDArray.FromValues(new double[,] { { 0, 0, 1, 2 }, { 0, 0, 5, 6 } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(NDArray.GetOneShifted(b, 1), 1), expected));

            expected = NDArray.FromValues(new double[,,] { { { 0, 0, 0 }, { 0, 0, 0 }, {0, 0, 0 } }, { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(c, 0), expected));
            expected = NDArray.FromValues(new double[,,] { { { 0, 0, 0 }, { 1, 2, 3 }, { 4, 5, 6 } }, { { 0, 0, 0 }, { 10, 11, 12 }, { 13, 14, 15 } } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(c, 1), expected));

            expected = NDArray.FromValues(new double[,,] { { { 0, 1, 2 }, { 0, 4, 5 }, { 0, 7, 8 } }, { { 0, 10, 11 }, { 0, 13, 14}, { 0, 16, 17 } } });
            Assert.True(NDArray.ArrayEqual(NDArray.GetOneShifted(c, 2), expected));
        }


    }
}
