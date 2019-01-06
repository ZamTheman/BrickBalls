using Collision2D.BasicGeometry;

namespace Collision2D.HelperObjects
{
    public static class DistanceHelper
    {
        public static bool WithinDistance(Point pt1, Point pt2, float length)
        {
            return (
                (pt2.X - pt1.X)
                * (pt2.X - pt1.X)
                + (pt2.Y - pt1.Y)
                * (pt2.Y - pt1.Y))
                < length * length;
        }
    }
}
