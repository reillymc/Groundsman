using System;
using System.Collections.Generic;
using System.IO;
using Groundsman.JSONConverters;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Groundsman.Models
{
    /// <summary>
    /// A Feature object represents a spatially bounded thing.  Every Feature
    /// object is a GeoJSON object no matter where it occurs in a GeoJSON text.
    /// </summary> 
    [JsonConverter(typeof(DummyConverter))]
    public class Feature : GeoJSONObject
    {
        [PrimaryKey]
        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        public string Author { get; set; }

        [TextBlob(nameof(GeometryBlobbed))]
        [JsonProperty(PropertyName = "geometry", Order = 2)]
        public Geometry Geometry { get; set; }
        public string GeometryBlobbed { get; set; } // serialized Geometry

        [TextBlob(nameof(PropertiesBlobbed))]
        [JsonProperty(PropertyName = "properties", Order = 3), JsonConverter(typeof(PropertiesConverter<string, object>))]
        public IDictionary<string, object> Properties { get; set; }
        public string PropertiesBlobbed { get; set; } // serialized Properties

        [JsonConstructor]
        public Feature(Geometry geometry = null, IDictionary<string, object> properties = null) : base(GeoJSONType.Feature)
        {
            Id = Guid.NewGuid().ToString();

            Geometry = geometry;

            Date = DateTime.Now.ToShortDateString();

            Properties = properties ?? new Dictionary<string, object>();

            // If author ID hasn't been set on the feature, default it to the user's ID.
            string author = "Unknown";
            if (Properties.ContainsKey(Constants.AuthorProperty))
            {
                author = (string)Properties[Constants.AuthorProperty];
                if (author.Length > 30)
                {
                    author = author.Substring(0, 30);
                }
            }
            Author = author;

            // Add default name if empty
            string name = Geometry.Type.ToString();
            if (Properties.ContainsKey(Constants.NameProperty))
            {
                name = (string)Properties[Constants.NameProperty];
                if (name.Length > 30)
                {
                    name = name.Substring(0, 30);
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        name = name.Replace(c, '-');
                    }
                }
            }
            Name = name;

            // If the date field is missing or invalid, convert it into DateTime.Now.
            DateTime date = DateTime.Now;
            if (Properties.ContainsKey(Constants.DateProperty))
            {
                DateTime.TryParse((string)Properties[Constants.DateProperty], out date);
            }
            Date = date.ToShortDateString();

            if (Id == null)
            {
                Id = Guid.NewGuid().ToString();
            }
        }

        public Feature() : base(GeoJSONType.Feature)
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
