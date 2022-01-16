using System;
using System.Collections.Generic;
using System.Globalization;
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
        // Safe property accessors for feature properties used directly by Groundsman.
        // TODO: move validation checks to these properties
        [PrimaryKey]
        [JsonIgnore]
        public string Id
        {
            get
            {
                if (Properties.TryGetValue(Constants.IdentifierProperty, out object value)) return (string)value;
                var newId = Guid.NewGuid().ToString();
                Properties[Constants.IdentifierProperty] = newId;
                return newId;
            }
            set => Properties[Constants.IdentifierProperty] = value;
        }

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Properties.TryGetValue(Constants.NameProperty, out object value)) return (string)value;
                if (Geometry != null) return Geometry.Type.ToString();
                return "Feature";
            }
            set => Properties[Constants.NameProperty] = value;
        }

        [JsonIgnore]
        public DateTime Date
        {
            get
            {
                if (Properties.TryGetValue(Constants.DateProperty, out object value) && DateTime.TryParseExact((string)value, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) return date;
                return DateTime.Now;
            }
            set
            {
                Properties[Constants.DateProperty] = value.ToShortDateString();
            }
        }

        [JsonIgnore]
        public string Author
        {
            get => Properties.TryGetValue(Constants.AuthorProperty, out object value) ? (string)value : Constants.DefaultUserValue;
            set
            {
                if (value.Length > 30)
                {
                    Properties[Constants.AuthorProperty] = value.Substring(0, 30);
                }
                else
                {
                    Properties[Constants.AuthorProperty] = value;
                }
            }
        }

        // Feature geomtery data. Stored as blobbed JSON internally by Groundsman but converted to Geometry object when read in.
        [TextBlob(nameof(GeometryBlobbed))]
        [JsonProperty(PropertyName = "geometry", Order = 2)]
        public Geometry Geometry { get; set; }
        public string GeometryBlobbed { get; set; }

        // Feature properties data. Stored as blobbed JSON internally by Groundsman but converted to Geometry object when read in.
        [TextBlob(nameof(PropertiesBlobbed))]
        [JsonProperty(PropertyName = "properties", Order = 3), JsonConverter(typeof(PropertiesConverter<string, object>))]
        public IDictionary<string, object> Properties { get; set; }
        public string PropertiesBlobbed { get; set; } // serialized Properties


        [JsonConstructor]
        public Feature(Geometry geometry = null, IDictionary<string, object> properties = null) : base(GeoJSONType.Feature)
        {

            Geometry = geometry;

            Properties = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                if (string.IsNullOrEmpty(property.Key.ToString())) break;

                try
                {
                    if (property.Key == Constants.IdentifierProperty) continue;

                    if (property.Key == Constants.NameProperty)
                    {
                        string value = (string)property.Value;
                        value = value.Length >= 20 ? value.Substring(0, 20) : value;
                        foreach (char c in Path.GetInvalidFileNameChars())
                        {
                            value = value.Replace(c, '-');
                        }
                        Name = value;
                        continue;
                    }

                    if (property.Key == Constants.DateProperty)
                    {
                        DateTime date = DateTime.Now;
                        DateTime.TryParse((string)property.Value, out date);
                        Date = date;
                        continue;
                    }

                    var propertyType = property.Value.GetType();

                    if (propertyType == typeof(string))
                    {
                        string value = (string)property.Value;
                        Properties[property.Key] = value.Length >= 400 ? value.Substring(0, 400) : value;
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(long))
                    {
                        Properties[property.Key] = property.Value;
                    }
                    else if (propertyType == typeof(float) || propertyType == typeof(double))
                    {
                        Properties[property.Key] = property.Value;
                    }
                    else if (propertyType == typeof(bool))
                    {
                        Properties[property.Key] = property.Value;
                    }
                    else
                    {
                        throw new ArgumentException($"Could not save unsupported property '{property.Key}'");
                    }
                }

                catch
                {
                    throw new ArgumentException($"{property.Key} '{property.Value}' is incorrectly formatted.", property.Key);
                }
            }
        }

        public Feature() : base(GeoJSONType.Feature)
        {
            Properties = new Dictionary<string, object>();
            Id = Guid.NewGuid().ToString();
        }
    }
}
