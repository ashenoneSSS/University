using Microsoft.VisualStudio.TestTools.UnitTesting;
using MatrixCore;
using System.Threading.Tasks;

namespace MatrixTests
{
    [TestClass]
    public class MatrixTests
    {
        [TestMethod]
        public async Task Multiply_2x2()
        {
            var a = new Matrix(2, 2);
            a[0, 0] = 1; a[0, 1] = 2;
            a[1, 0] = 3; a[1, 1] = 4;

            var b = new Matrix(2, 2);
            b[0, 0] = 5; b[0, 1] = 6;
            b[1, 0] = 7; b[1, 1] = 8;

            var mult = new MatrixMultiplier();
            var result = await mult.ExecuteAsync(a, b);

            Assert.AreEqual(19, result[0, 0]);
            Assert.AreEqual(22, result[0, 1]);
            Assert.AreEqual(43, result[1, 0]);
            Assert.AreEqual(50, result[1, 1]);
        }

        [TestMethod]
        public async Task Determinant_2x2()
        {
            var a = new Matrix(2, 2);
            a[0, 0] = 1; a[0, 1] = 2;
            a[1, 0] = 3; a[1, 1] = 4;

            var det = new DeterminantCalculator();
            var result = await det.ExecuteAsync(a);

            Assert.AreEqual(-2, result[0, 0], 1e-9);
        }

        [TestMethod]
        public async Task Inverse_2x2()
        {
            var a = new Matrix(2, 2);
            a[0, 0] = 4; a[0, 1] = 7;
            a[1, 0] = 2; a[1, 1] = 6;

            var inv = new MatrixInverter();
            var result = await inv.ExecuteAsync(a);

            Assert.AreEqual(0.6, result[0, 0], 1e-9);
            Assert.AreEqual(-0.7, result[0, 1], 1e-9);
            Assert.AreEqual(-0.2, result[1, 0], 1e-9);
            Assert.AreEqual(0.4, result[1, 1], 1e-9);
        }
    }
}
