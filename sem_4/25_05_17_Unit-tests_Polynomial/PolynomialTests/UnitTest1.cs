using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PolynomialTests
{
    [TestClass]
    public class PolynomialTests
    {
        private static string PolyString(Polynomial p)
        {
            var sw = new System.IO.StringWriter();
            Console.SetOut(sw);
            p.Display();
            return sw.ToString().Trim();
        }

        [TestMethod]
        public void Insert_InCorrectOrder()
        {
            var poly = new Polynomial();
            poly.Insert(3, 1);
            poly.Insert(5, 3);
            poly.Insert(2, 2);
            Assert.AreEqual("5x^3 + 2x^2 + 3x^1", PolyString(poly));
        }

        [TestMethod]
        public void AddTerm_MergeCoefficients()
        {
            var poly = new Polynomial();
            poly.Insert(4, 2);
            poly.AddTerm(3, 2);
            Assert.AreEqual("7x^2", PolyString(poly));
        }

        [TestMethod]
        public void Add_ReturnsSum()
        {
            var p1 = new Polynomial();
            p1.Insert(2, 1);
            var p2 = new Polynomial();
            p2.Insert(5, 1);

            var sum = p1.Add(p2);
            Assert.AreEqual("7x^1", PolyString(sum));
        }

        [TestMethod]
        public void Subtract_ReturnsDifference()
        {
            var p1 = new Polynomial();
            p1.Insert(7, 0);
            var p2 = new Polynomial();
            p2.Insert(3, 0);

            var diff = p1.Subtract(p2);
            Assert.AreEqual("4x^0", PolyString(diff));
        }

        [TestMethod]
        public void Multiply_ReturnsProduct()
        {
            var p1 = new Polynomial();
            p1.Insert(2, 1);          
            var p2 = new Polynomial();
            p2.Insert(3, 2);          

            var prod = p1.Multiply(p2); 
            Assert.AreEqual("6x^3", PolyString(prod));
        }
    }
}
