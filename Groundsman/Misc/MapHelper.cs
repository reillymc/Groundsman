using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Internals;
using XFMPosition = Xamarin.Forms.Maps.Position;
using XFMPolygon = Xamarin.Forms.Maps.Polygon;
using Groundsman.Models;
using Position = Groundsman.Models.Position;
using Point = Groundsman.Models.Point;
using Polygon = Groundsman.Models.Polygon;

namespace Groundsman.Misc;

public static class MapHelper
{
    public static Pin GeneratePin(Feature feature)
    {
        Point point = (Point)feature.Geometry;
        string address = double.IsNaN(point.Coordinates.Altitude)
            ? $"{point.Coordinates.Longitude}, {point.Coordinates.Latitude}"
            : $"{point.Coordinates.Longitude}, {point.Coordinates.Latitude}, {point.Coordinates.Altitude}";
        Pin pin = new Pin
        {
            Label = feature.Name,
            Address = address,
            Type = PinType.Place,
            Position = new XFMPosition(point.Coordinates.Latitude, point.Coordinates.Longitude),
        };
        return pin;
    }

    public static Polyline GenerateLine(Feature feature)
    {
        Polyline polyline = new Polyline
        {
            StrokeColor = Color.OrangeRed,
            StrokeWidth = 5,
        };
        LineString lineString = (LineString)feature.Geometry;
        lineString.Coordinates.ForEach((Position point) =>
        {
            polyline.Geopath.Add(new XFMPosition(point.Latitude, point.Longitude));
        });
        return polyline;
    }

    public static XFMPolygon GeneratePolygon(Feature feature)
    {
        XFMPolygon xfmpolygon = new XFMPolygon
        {
            StrokeWidth = 4,
            StrokeColor = Color.OrangeRed,
            FillColor = Color.OrangeRed.AddLuminosity(.1).MultiplyAlpha(0.6),
        };

        Polygon polygon = (Polygon)feature.Geometry;
        foreach (LineString lineString in polygon.Coordinates)
        {
            foreach (Position pos in lineString.Coordinates)
            {
                xfmpolygon.Geopath.Add(new XFMPosition(pos.Latitude, pos.Longitude));
            }
        }
        return xfmpolygon;
    }
}
