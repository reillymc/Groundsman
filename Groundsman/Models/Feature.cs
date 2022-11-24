using System.Globalization;
using Groundsman.Helpers;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// A Feature object represents a spatially bounded thing.  Every Feature
/// object is a GeoJSON object no matter where it occurs in a GeoJSON text.
/// </summary> 
[JsonConverter(typeof(DummyConverter))]
public class Feature : GeoJSONObject
{
    // Safe property accessors for feature properties used directly by Groundsman.
    [JsonIgnore]
    public string Id
    {
        get
        {
            if (Properties.TryGetValue(DefaultProperties.Id, out object value)) return (string)value;
            var newId = Guid.NewGuid().ToString();
            Properties[DefaultProperties.Id] = newId;
            return newId;
        }
        set => Properties[DefaultProperties.Id] = value;
    }

    [JsonIgnore]
    public string Name
    {
        get
        {
            if (Properties.TryGetValue(DefaultProperties.Name, out object value)) return (string)value;
            if (Geometry != null) return Geometry.Type.ToString();
            return "Feature";
        }
        set => Properties[DefaultProperties.Name] = value;
    }

    [JsonIgnore]
    public DateTime Date
    {
        get
        {
            if (Properties.TryGetValue(DefaultProperties.Date, out object value) && DateTime.TryParseExact((string)value, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) return date;
            return DateTime.Now;
        }
        set
        {
            Properties[DefaultProperties.Date] = value.ToShortDateString();
        }
    }

    [JsonIgnore]
    public string Author
    {
        get => Properties.TryGetValue(DefaultProperties.Author, out object value) ? (string)value : Constants.DefaultUserId;
        set
        {
            if (value.Length > 30)
            {
                Properties[DefaultProperties.Author] = value.Substring(0, 30);
            }
            else
            {
                Properties[DefaultProperties.Author] = value;
            }
        }
    }

    [JsonProperty(PropertyName = "geometry", Order = 2)]
    public Geometry Geometry { get; set; }

    [JsonProperty(PropertyName = "properties", Order = 3), JsonConverter(typeof(PropertiesConverter<string, object>))]
    public IDictionary<string, object> Properties { get; set; }

    [JsonConstructor]
    public Feature(Geometry geometry = null, IDictionary<string, object> properties = null) : base(GeoJSONType.Feature)
    {
        Geometry = geometry;

        Properties = new Dictionary<string, object>();

        if (properties == null) return;

        foreach (var property in properties)
        {
            if (string.IsNullOrEmpty(property.Key.ToString())) break;

            try
            {
                if (property.Key == DefaultProperties.Id)
                {
                    if (Guid.TryParse((string)property.Value, out Guid id)) Id = id.ToString();
                }

                if (property.Key == DefaultProperties.Name)
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

                if (property.Key == DefaultProperties.Date)
                {
                    DateTime date = DateTime.Now;
                    DateTime.TryParse((string)property.Value, out date);
                    Date = date;
                    continue;
                }

                if (property.Key == DefaultProperties.Timestamps)
                {
                    Properties[DefaultProperties.Timestamps] = property.Value;
                    continue;
                }

                var propertyType = property.Value.GetType();

                if (propertyType == typeof(string))
                {
                    string value = (string)property.Value;
                    Properties[property.Key] = value.Length >= 400 ? value.Substring(0, 400) : value;
                }
                else if (propertyType == typeof(short) || propertyType == typeof(int) || propertyType == typeof(long))
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
