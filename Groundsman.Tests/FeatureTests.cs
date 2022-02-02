using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Groundsman.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Groundsman.Tests
{
    [TestClass]
    public class FeatureTests
    {
        /// <summary>
        /// Try to create a regular Point Feature with all property types
        /// </summary>
        [TestMethod]
        public void CreatePoint()
        {
            Feature point = (Feature)GeoJSONObject.ImportGeoJSON(File.ReadAllText("point.json"));

            var importCount = point.Properties.Count;
            var existingCount = TestData.PointPropertySet.Count;

            var countEqual = importCount == existingCount;
            var exceptions = point.Properties.Except(TestData.PointPropertySet);

            Assert.IsTrue(point.Properties["author"].ToString() == TestData.PointPropertySet["author"].ToString());
            Assert.IsTrue(DateTime.Parse(point.Properties["date"].ToString()) == DateTime.Parse(TestData.PointPropertySet["date"].ToString()));
            Assert.IsTrue(point.Properties["Default String Property"].ToString() == TestData.PointPropertySet["Default String Property"].ToString());
            Assert.IsTrue(Convert.ToInt32(point.Properties["Default Integer Property"]) == Convert.ToInt32(TestData.PointPropertySet["Default Integer Property"]));
            Assert.IsTrue(Convert.ToSingle(point.Properties["Default Float Property"]) == Convert.ToSingle(TestData.PointPropertySet["Default Float Property"]));
            Assert.IsTrue(Convert.ToBoolean(point.Properties["favourite"]) == Convert.ToBoolean(TestData.PointPropertySet["favourite"]));
            Assert.IsTrue(point.Properties["name"].ToString() == TestData.PointPropertySet["name"].ToString());
            Assert.IsTrue(countEqual && !exceptions.Any());
        }

        /// <summary>
        /// Try to create a regular Point Feature with all property types
        /// </summary>
        [TestMethod]
        public void SaveAndCheckPoint()
        {
            Point point = new Point(new Position(0, 0));
            Feature feature = new Feature(point, TestData.PointPropertySet);

            string export = GeoJSONObject.ExportGeoJSON(feature);

            Feature importFeature = (Feature)GeoJSONObject.ImportGeoJSON(export);
            Point importPoint = (Point)importFeature.Geometry;

            Assert.AreEqual(feature.Properties.Count, importFeature.Properties.Count); // TODO: proper deep compare of properties
            Assert.IsTrue(point == importPoint);
        }
    }
}
