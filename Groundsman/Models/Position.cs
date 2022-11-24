using Groundsman.Helpers;
using Newtonsoft.Json;

namespace Groundsman.Models;

/// <summary>
/// A position is the fundamental geometry construct. An array of two - three values
/// </summary>
[JsonConverter(typeof(PositionConverter))]
public record Position
{
    [JsonProperty(PropertyName = "longitude", Order = 2)]
    public double Longitude { get; init; }

    [JsonProperty(PropertyName = "latitude", Order = 2)]
    public double Latitude { get; init; }

    [JsonProperty(PropertyName = "altitude", Order = 2)]
    public double Altitude { get; init; }

    public Position(double longitude, double latitude) : this(longitude, latitude, double.NaN) { }

    [JsonConstructor]
    public Position(double longitude, double latitude, double altitude)
    {
        if (double.IsNaN(longitude) || double.IsInfinity(longitude))
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be a valid number.");
        }

        if (double.IsNaN(latitude) || double.IsInfinity(latitude))
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Longitude must be a valid number.");
        }

        Longitude = longitude;
        Latitude = latitude;
        Altitude = double.IsInfinity(altitude) ? double.NaN : altitude;
    }

    public Position(DisplayPosition displayPosition)
    {
        try
        {
            Longitude = (string.IsNullOrEmpty(displayPosition.Longitude) || displayPosition.Longitude == "-") ? 0 : Convert.ToDouble(displayPosition.Longitude);
            if (double.IsInfinity(Longitude))
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        catch
        {
            throw new ArgumentOutOfRangeException(nameof(displayPosition.Longitude), "Longitude must be a valid number.");
        }
        try
        {
            Latitude = (string.IsNullOrEmpty(displayPosition.Latitude) || displayPosition.Latitude == "-") ? 0 : Convert.ToDouble(displayPosition.Latitude);
            if (double.IsInfinity(Longitude))
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        catch
        {
            throw new ArgumentOutOfRangeException(nameof(displayPosition.Latitude), "Longitude must be a valid number.");
        }
        try
        {
            Altitude = string.IsNullOrEmpty(displayPosition.Altitude) ? double.NaN : Convert.ToDouble(displayPosition.Altitude);
            if (double.IsInfinity(Altitude))
            {
                Altitude = 0;
            }
        }
        catch
        {
            throw new ArgumentOutOfRangeException(nameof(displayPosition.Altitude), "Altitude must be a valid number or blank.");
        }
    }

    public bool HasAltitude() => !double.IsNaN(Altitude);
}
