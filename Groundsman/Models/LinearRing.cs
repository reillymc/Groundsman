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
            Position[] coords = Coordinates.ToArray();

            if (coords.Length < 4)
            {
                throw new ArgumentOutOfRangeException("Coordinates", "A polygon's linear ring must have four or more positions.");
            }

            if (!coords.First().Equals(coords.Last()))
            {
                throw new ArgumentException("The first and last values of a polygon's linear ring must be identiacal.", "Coordinates");
            }
        }
    }
}
