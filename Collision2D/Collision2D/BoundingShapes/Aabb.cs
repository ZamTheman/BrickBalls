using Collision2D.BasicGeometry;
using System;
using System.Collections.Generic;

namespace Collision2D.BoundingShapes
{
    public class Aabb
    {
        public Point Center { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Left { get => Center.X - Width * 0.5f; }
        public float Right { get => Center.X + Width * 0.5f; }
        public float Top { get => Center.Y - Height * 0.5f; }
        public float Bottom { get => Center.Y + Height * 0.5f; }
        public float DistanceCenterToCorner;
        
        public Aabb()
        {
            Center = new Point();
        }

        public Aabb(Point center, float width, float height)
        {
            Center = center;
            Width = width;
            Height = height;
            DistanceCenterToCorner = (float)Math.Sqrt(
                Width * 0.5 * Width * 0.5 +
                Height * 0.5 * Height * 0.5);
    }

        public Aabb(Point start, Point end)
        {
            Center = new Point((end.X - start.X) * 0.5f, (end.Y - start.Y) * 0.5f);
            Width = end.X - start.X;
            Height = end.Y - start.Y;
            DistanceCenterToCorner = (float)Math.Sqrt(
                Width * 0.5 * Width * 0.5 +
                Height * 0.5 * Height * 0.5);
        }

        public Aabb(float startX, float startY, float width, float height)
        {
            Center = new Point(startX + width * 0.5f, startY + height * 0.5f);
            Width = width;
            Height = height;
            DistanceCenterToCorner = (float)Math.Sqrt(
                Width * 0.5 * Width * 0.5 +
                Height * 0.5 * Height * 0.5);
        }

        public Point GetClosestCorner(Point intersectionPoint)
        {
            var Corners = new List<Point>();
            Corners.Add(new Point(Left, Top));
            Corners.Add(new Point(Right, Top));
            Corners.Add(new Point(Right, Bottom));
            Corners.Add(new Point(Left, Bottom));

            var closestCorner = new Point(float.MaxValue, float.MaxValue);
            var closestDistance = float.MaxValue;
            foreach (var corner in Corners)
            {
                var distance = intersectionPoint.Distance(corner);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCorner = corner;
                }
            }

            return closestCorner;
        }
    }
}
