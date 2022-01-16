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

            var countEqal = point.Properties.Count == TestData.PointPropertySet.Count;
            var exceptions = point.Properties.Except(TestData.PointPropertySet);

            Assert.IsTrue(countEqal && !exceptions.Any());
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

            Assert.AreEqual(feature.Properties.Count, importFeature.Properties.Count - 1); // TODO: proper deep compare of properties
            Assert.IsTrue(point.Equals(importPoint));
        }
    }
}
