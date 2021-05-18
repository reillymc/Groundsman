using System;
using System.Collections.Generic;
using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class PolygonTests
    {
        private Polygon GetSingleLRPolygon() => new Polygon(new List<LinearRing>() { new LinearRing(new List<Position> { TestData.Position1, new Position(40, 40), new Position(20, 40), TestData.Position3, TestData.Position1 }) });

        private Polygon GetDoubleLRPolygon() => new Polygon(new List<LinearRing>() { new LinearRing(new List<Position> { TestData.Position1, new Position(45, 45), new Position(15, 40), TestData.Position3, TestData.Position1 }), new LinearRing(new List<Position> { new Position(20, 30), new Position(35, 35), new Position(30, 20), new Position(20, 30) }) });

        private Position GetPosition(double lon, double lat) => new Position(lon, lat);


        /// <summary>
        /// Try to create a Polygon without giving coordinates
        /// </summary>
        [TestMethod]
        public void CreatePolygonWithNull() => Assert.ThrowsException<ArgumentNullException>(() => { Polygon polygon = new Polygon(null); });

        /// <summary>
        /// Try to create a Polygon with an empty list of LinearRings
        /// </summary>
        [TestMethod]
        public void CreatePolygonWithZeroLinearRings() => Assert.ThrowsException<ArgumentException>(() => { Polygon polygon = new Polygon(new List<LinearRing>()); });

        /// <summary>
        /// Try to create a regular Polygon with a single linear ring
        /// </summary>
        [TestMethod]
        public void CreatePolygonWithOneLinearRings()
        {
            Polygon polygon = new Polygon(GetSingleLRPolygon().Coordinates);

            List<LinearRing> coords = (List<LinearRing>)polygon.Coordinates;

            Assert.AreEqual(coords.Count, 1);
        }

        /// <summary>
        /// Try to create a Polygon with two linear rings
        /// </summary>
        [TestMethod]
        public void CreatePolygonWithTwoLinearRings()
        {
            Polygon polygon = new Polygon(GetDoubleLRPolygon().Coordinates);

            List<LinearRing> coords = (List<LinearRing>)polygon.Coordinates;

            Assert.AreEqual(coords.Count, 2);
        }

        /// <summary>
        /// Verify that a point is within a polygon of a single LinearRing
        /// </summary>
        [TestMethod]
        public void PointInSingleLayerPolygon()
        {
            Polygon polygon = GetSingleLRPolygon();

            bool contains = polygon.ContainsPosition(GetPosition(26, 28));

            Assert.IsTrue(contains);
        }

        /// <summary>
        /// Verify that a point is outside a polygon of a single LinearRing
        /// </summary>
        [TestMethod]
        public void PointNotInSingleLayerPolygon()
        {
            Polygon polygon = GetSingleLRPolygon();

            bool contains = polygon.ContainsPosition(GetPosition(35, 23));

            Assert.IsFalse(contains);
        }

        /// <summary>
        /// Verify that a point is in polygon of two LinearRings. The point is inside the first LinearRing but not in the second (which exclused the area within from the polygon), therefore it is contained in the polygon
        /// </summary>
        [TestMethod]
        public void PointInDoubleLayerPolygon()
        {
            Polygon polygon = GetDoubleLRPolygon();

            bool contains = polygon.ContainsPosition(GetPosition(35, 23));

            Assert.IsTrue(contains);
        }

        /// <summary>
        /// Verify that a point is not in polygon of two LinearRings. The point is within both LinearRings, therefore it is not contained in the polygon
        /// </summary>
        [TestMethod]
        public void PointInOutDoubleLayerPolygon()
        {
            Polygon polygon = GetDoubleLRPolygon();

            bool contains = polygon.ContainsPosition(GetPosition(26, 28));

            Assert.IsFalse(contains);
        }

        /// <summary>
        /// Verify that a point is entirely outside a polygon of a two LinearRings
        /// </summary>
        [TestMethod]
        public void PointNotInDoubleLayerPolygon()
        {
            Polygon polygon = GetDoubleLRPolygon();

            bool contains = polygon.ContainsPosition(GetPosition(14, 17));

            Assert.IsFalse(contains);
        }
    }
}
