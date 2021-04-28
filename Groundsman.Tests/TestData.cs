using System.Collections.Generic;
using Groundsman.Models;

namespace Groundsman.Tests
{
    public class TestData
    {
        public static Position Position1 => new Position(30, 10);
        public static Position Position2 => new Position(20, 10);
        public static Position Position3 => new Position(10, 20);
        public static Position Position4 => new Position(30, 10);
        public static Position Position5 => new Position(30, 11);

        public static List<Position> PositionList1 => new List<Position>() { Position1, Position2, Position3, Position4 };
    }
}
