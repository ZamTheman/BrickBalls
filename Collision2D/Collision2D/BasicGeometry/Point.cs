using System;

namespace Collision2D.BasicGeometry
{
    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point() { }
        public Point(float x, float y) { X = x; Y = y; }

        public float Distance(Point point)
        {
            float distX = Math.Abs(point.X - X);
            float distY = Math.Abs(point.Y - Y);

            return (float)Math.Sqrt(distX * distX + distY * distY);
        }

        public static Point operator *(Point pt, float value)
        {
            return new Point(pt.X * value, pt.Y * value);
        }

        public static Point operator +(Point pt1, Point pt2)
        {
            return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
        }

        public static Point operator +(Point pt, Vector vec)
        {
            return new Point(pt.X + vec.X, pt.Y + vec.Y);
        }

        public static Point operator -(Point pt1, Point pt2)
        {
            return new Point(pt1.X - pt2.X, pt1.Y - pt2.Y);
        }
    }
}
