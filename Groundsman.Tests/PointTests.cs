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
        public void CreatePointWithNull() => Assert.ThrowsException<ArgumentNullException>(() => { Point point = new Point(null); });

        /// <summary>
        /// Try to create a regular Point
        /// </summary>
        [TestMethod]
        public void CreatePointWithOnePosition()
        {
            Point point = new Point(TestData.Position1);

            Assert.IsTrue(point.Coordinates.Equals(TestData.Position1));
        }


        /// <summary>
        /// Check that two points return true for equality checks
        /// </summary>
        [TestMethod]
        public void CheckPositionEquality()
        {
            Point point = new Point(new Position(10, 20, 30));
            Point point2 =  new Point(new Position(10, 20, 30));

            Assert.AreEqual(point.Coordinates, point2.Coordinates);
            Assert.IsTrue(point.Equals(point2));
            Assert.IsTrue(point == point2);
        }
    }
}
