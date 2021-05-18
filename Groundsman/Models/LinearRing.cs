using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Groundsman.Models
{
    /// <summary>
    /// A supporting type used to create polygons
    /// </summary>
    public class LinearRing : LineString
    {
        [JsonConstructor]
        public LinearRing(IEnumerable<Position> coordinates) : base(coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException(nameof(coordinates), "A LinearRing must have coordinates.");
            }

            if (Coordinates.ToArray().Length < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(coordinates), "A polygon's linear ring must have four or more positions.");
            }

            if (!coordinates.First().Equals(coordinates.Last()))
            {
                throw new ArgumentException("The first and last values of a polygon's linear ring must be identiacal.", nameof(coordinates));
            }
        }
    }
}
