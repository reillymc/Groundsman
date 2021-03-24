namespace Groundsman.Models
{
    public enum GeoJSONType
    {
        //Geometry Types
        Point,
        MultiPoint, //Not currently implemented
        LineString,
        MultiLineString, //Not currently implemented
        Polygon,
        MultiPolygon, //Not currently implemented
        GeometryCollection, //Not currently implemented

        //GeoJSON types
        Feature,
        FeatureCollection
    }
}
