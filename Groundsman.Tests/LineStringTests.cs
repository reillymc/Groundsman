using System;
using System.Collections.Generic;
using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class LineStringTests
    {
        private LineString GetLineString() => new LineString(new List<Position> { TestData.Position1, TestData.Position3 });

        private Position GetLiesOnLinePosition() => new Position(20, 15);
        private Position GetNotLiesOnLinePosition() => new Position(15, 15); //TODOL List of different bad psotions?

        /// <summary>
        /// Try to create a LineString without giving coordinates
        /// </summary>
        [TestMethod]
        public void CreateLineStringWithNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => { LineString lineString = new LineString(null); });
        }

        /// <summary>
        /// Try to create a LineString with an empty list of Positions
        /// </summary>
        [TestMethod]
        public void CreateLineStringWithZeroPositions()
        {
            Assert.ThrowsException<ArgumentException>(() => { LineString lineString = new LineString(new List<Position>()); });
        }

        /// <summary>
        /// Try to create a LineString with a list of one Position
        /// </summary>
        [TestMethod]
        public void CreateLineStringWithOnePosition()
        {
            Assert.ThrowsException<ArgumentException>(() => { LineString lineString = new LineString(new List<Position>() { { TestData.Position1 } }); });
        }

        /// <summary>
        /// Try to create a regular LineString with a list of two positions
        /// </summary>
        [TestMethod]
        public void CreateLineStringWithTwoPositions()
        {
            LineString lineString = GetLineString();

            List<Position> coords = (List<Position>)lineString.Coordinates;

            Assert.AreEqual(coords.Count, 2);
        }

        /// <summary>
        /// A point lies on a LineString
        /// </summary>
        [TestMethod]
        public void PointOnLineString()
        {
            LineString lineString = GetLineString();

            bool contains = lineString.ContainsPosition(GetLiesOnLinePosition());

            Assert.IsTrue(contains);
        }

        /// <summary>
        /// A point does not lie on a LineString
        /// </summary>
        [TestMethod]
        public void PointNotOnLineString()
        {
            LineString lineString = GetLineString();

            bool contains = lineString.ContainsPosition(GetNotLiesOnLinePosition());

            Assert.IsFalse(contains);
        }
    }
}