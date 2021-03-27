using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class LineStringTests
    {
        private LineString GetLineString()
        {
            return LineString.ImportGeoJSON("{\"type\":\"LineString\",\"coordinates\":[[153.03150415420532, -27.477759278724388],[153.0296802520752,-27.479339341068858]]}");
        }

        /// <summary>
        /// A point lies on a LineString
        /// </summary>
        [TestMethod]
        public void PointOnLineString()
        {
            LineString lineString = GetLineString();

            bool contains = lineString.ContainsPosition(new Position(153.030135557055, -27.4787212395597));

            Assert.IsTrue(contains);
        }

        /// <summary>
        /// A point does not lie on a LineString
        /// </summary>
        [TestMethod]
        public void PointNotOnLineString()
        {
            LineString lineString = GetLineString();

            bool contains = lineString.ContainsPosition(new Position(154, -28));

            Assert.IsFalse(contains);
        }

    }
}
