using System;
using System.Collections.Generic;
using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class LinearRingTests
    {
        /// <summary>
        /// Try to create a LinearRing without giving coordinates
        /// </summary>
        [TestMethod]
        public void CreateLinearRingWithNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => { LinearRing linearRing = new LinearRing(null); });
        }

        /// <summary>
        /// Try to create a LinearRing with an empty list of Positions
        /// </summary>
        [TestMethod]
        public void CreateLinearRingWithPositionsRings()
        {
            Assert.ThrowsException<ArgumentException>(() => { LinearRing linearRing = new LinearRing(new List<Position>()); });
        }

        /// <summary>
        /// Try to create a LinearRing with a list of three Positions
        /// </summary>
        [TestMethod]
        public void CreateLinearRingWithThreePositions()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { LinearRing linearRing = new LinearRing(new List<Position>() { TestData.Position1, TestData.Position2, TestData.Position3 }); });
        }

        /// <summary>
        /// Try to create a LinearRing with mismatching first and last Positions
        /// </summary>
        [TestMethod]
        public void CreateLinearRingWithMismatchingPositions()
        {
            Assert.ThrowsException<ArgumentException>(() => { LinearRing linearRing = new LinearRing(new List<Position>() { TestData.Position1, TestData.Position2, TestData.Position3, TestData.Position5 }); });
        }

        /// <summary>
        /// Try to create a regular LinearRing
        /// </summary>
        [TestMethod]
        public void CreateLinearRingPositions()
        {
            LinearRing linearRing = new LinearRing(TestData.PositionList1);

            List<Position> coords = (List<Position>)linearRing.Coordinates;

            Assert.AreEqual(coords.Count, 4);
        }

    }
}
