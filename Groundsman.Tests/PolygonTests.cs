using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class PolygonTests
    {
        private Polygon GetSingleLRPolygon()
        {
            return Polygon.ImportGeoJSON("{\"type\":\"Polygon\",\"coordinates\":[[[30,10],[40,40],[20,40],[10,20],[30,10]]]}");
            //return new Polygon(new List<LinearRing>() { new LinearRing(new List<Position> { new Position(30, 10), new Position(40, 40), new Position(20, 40), new Position(10, 20), new Position(30, 10) }) });
        }

        private Polygon GetTwoLRPolygon()
        {
            return Polygon.ImportGeoJSON("{\"type\":\"Polygon\",\"coordinates\":[[[35,10],[45,45],[15,40],[10,20],[35,10]],[[20,30],[35,35],[30,20],[20,30]]]}");
            //return new Polygon(new List<LinearRing>() { new LinearRing(new List<Position> { new Position(35, 10), new Position(45, 45), new Position(15, 40), new Position(10, 20), new Position(35, 10) }), new LinearRing(new List<Position> { new Position(20, 30), new Position(35, 35), new Position(30, 20), new Position(20, 30) }) });
        }

        /// <summary>
        /// A point is within a polygon of a single LinearRing
        /// </summary>
        [TestMethod]
        public void PointInSingleLayerPolygon()
        {
            Polygon polygon = GetSingleLRPolygon();

            bool contains = polygon.ContainsPosition(new Position(26, 28));

            Assert.IsTrue(contains);
        }

        /// <summary>
        /// A point is outside a polygon of a single LinearRing
        /// </summary>
        [TestMethod]
        public void PointNotInSingleLayerPolygon()
        {
            Polygon polygon = GetSingleLRPolygon();

            bool contains = polygon.ContainsPosition(new Position(35, 23));

            Assert.IsFalse(contains);
        }

        /// <summary>
        /// A point in polygon of two LinearRings. The point is inside the first LinearRing but not in the second (which exclused the area within from the polygon), therefore it is contained in the polygon
        /// </summary>
        [TestMethod]
        public void PointInDoubleLayerPolygon()
        {
            Polygon polygon = GetTwoLRPolygon();

            bool contains = polygon.ContainsPosition(new Position(35, 23));

            Assert.IsTrue(contains);
        }

        /// <summary>
        /// A point in polygon of two LinearRings. The point is within both LinearRings, therefore it is not contained in the polygon
        /// </summary>
        [TestMethod]
        public void PointInOutDoubleLayerPolygon()
        {
            Polygon polygon = GetTwoLRPolygon();

            bool contains = polygon.ContainsPosition(new Position(26, 28));

            Assert.IsFalse(contains);
        }

        /// <summary>
        /// A point is entirely outside a polygon of a two LinearRings
        /// </summary>
        [TestMethod]
        public void PointNotInDoubleLayerPolygon()
        {
            Polygon polygon = GetTwoLRPolygon();

            bool contains = polygon.ContainsPosition(new Position(14, 17));

            Assert.IsFalse(contains);
        }
    }
}
