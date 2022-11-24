using Groundsman.Models;

namespace Groundsman.Helpers;

public static class DefaultFeatures
{
    public static readonly Feature DefaultPoint = new(
        new Models.Point(
            new Position(153.028307, -27.477188)),
        new Dictionary<string, object>() {
            {
                "name",
                "Sample Point"
            }, {
                "author",
                "Groundsman"
            }, {
                "String Property",
                "This is a sample point"
            }, {
                "Integer Property",
                3
            }, {
                "Float Property",
                3.3
            },
        });

    public static readonly Feature DefaultLine = new(
        new LineString(
            new List<Position> {
                new Position(153.031504, -27.477759),
                new Position(153.029680, -27.479339),
            }),
        new Dictionary<string, object>() {
            {
                "name",
                "Test Line"
            }, {
                "author",
                "Groundsman"
            }, {
                "String Property",
                "This is a test line"
            }, {
                "Integer Property",
                2
            }, {
                "Float Property",
                2.2
            },
        }
    );

    public static readonly Feature DefaultPolygon = new(
        new Polygon(
            new List<LinearRing> {
                new LinearRing(
                    new List <Position> {
                        new Position(153.027449, -27.474751),
                        new Position(153.031375, -27.477283),
                        new Position(153.030560, -27.472276),
                        new Position(153.027449, -27.474751),
                    }
                ),
            }
        ),
        new Dictionary<string, object>() {
            {
                "name",
                "Example Polygon"
            }, {
                "author",
                "Groundsman"
            }, {
                "String Property",
                "This is an example polygon"
            }, {
                "Integer Property",
                1
            }, {
                "Float Property",
                1.1
            },
        }
    );

    public static readonly List<Feature> DefaultFeatureList = new() {
        DefaultPoint,
        DefaultLine,
        DefaultPolygon
    };
}