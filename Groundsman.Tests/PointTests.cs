using System;
using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class PointTests
    {
        /// <summary>
        /// Try to create a Point without giving coordinates
        /// </summary>
        [TestMethod]
        public void CreatePointWithNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => { Point point = new Point(null); });
        }

        /// <summary>
        /// Try to create a regular Point
        /// </summary>
        [TestMethod]
        public void CreatePointWithOnePosition()
        {
            Point point = new Point(TestData.Position1);

            Assert.IsTrue(point.Coordinates.Equals(TestData.Position1));
        }
    }
}
