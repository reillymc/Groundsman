using System;
using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class PositionTests
    {
        private DisplayPosition InvalidDisplayPosition => new DisplayPosition("0", "Test", "20", "30");
        private DisplayPosition ValidDisplayPosition => new DisplayPosition("0", "10", "20", "30");

        /// <summary>
        /// Try to create a Position with no longitude
        /// </summary>
        [TestMethod]
        public void CreatePositionWithNaNLongitude() => Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Position position = new Position(double.NaN, 10); });

        /// <summary>
        /// Try to create a Position with no latitude
        /// </summary>
        [TestMethod]
        public void CreatePositionWithNaNLatitude() => Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Position position = new Position(10, double.NaN); });

        /// <summary>
        /// Try to create a Position with no altitude
        /// </summary>
        [TestMethod]
        public void CreatePositionWithNullAltitude()
        {
            Position position = new Position(10, 10);

            Assert.AreEqual(position.Altitude, double.NaN);
        }

        /// <summary>
        /// Try to create a regular Position
        /// </summary>
        [TestMethod]
        public void CreatePositionWithOnePosition()
        {
            Position position = new Position(10, 20, 30);

            Assert.AreEqual(position.Longitude, 10);
            Assert.AreEqual(position.Latitude, 20);
            Assert.AreEqual(position.Altitude, 30);
        }

        /// <summary>
        /// Try to create a Position without giving a DisplayPosition
        /// </summary>
        [TestMethod]
        public void CreatePositionWithNull() => Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Position position = new Position(null); });

        /// <summary>
        /// Try to create a Position with an invalid DisplayPosition
        /// </summary>
        [TestMethod]
        public void CreatePositionWithInvalidDisplayPosition() => Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Position position = new Position(InvalidDisplayPosition); });

        /// <summary>
        /// Try to create a Position with a DisplayPosition
        /// </summary>
        [TestMethod]
        public void CreatePositionWithDisplayPosition()
        {
            Position position = new Position(ValidDisplayPosition);

            Assert.AreEqual(position.Longitude, 10);
            Assert.AreEqual(position.Latitude, 20);
            Assert.AreEqual(position.Altitude, 30);
        }

        /// <summary>
        /// Check that two positions return true for equality checks
        /// </summary>
        [TestMethod]
        public void CheckPositionEquality()
        {
            Position position = new Position(10, 20, 30);
            Position position2 = new Position(10, 20, 30);

            Assert.AreEqual(position, position2);
            //Assert.IsTrue(position == position2);
            Assert.IsTrue(position.Equals(position2));
        }
    }
}
