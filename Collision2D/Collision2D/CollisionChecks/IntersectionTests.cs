using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using Collision2D.HelperObjects;
using System;

namespace Collision2D.CollisionChecks
{
    public static class IntersectionTests
    {
        private const float EPSILON = 0.001f;

        public static IntersectionObject LinesegmentToAabbIntersection(LineSegment line, Aabb rect)
        {
            var fractionMin = 0f;
            var fractionMax = float.MaxValue;
            var displacementVector = line.Vector;
            var intersectionobject = new IntersectionObject() { Intersects = false };

            if(Math.Abs(displacementVector.X) < EPSILON)
            {
                if(line.Start.X < rect.Left || line.Start.X > rect.Right)
                    return intersectionobject;
            }

            if (Math.Abs(displacementVector.Y) < EPSILON)
            {
                if (line.Start.Y < rect.Top || line.Start.Y > rect.Bottom)
                    return intersectionobject;
            }

            var oodX = 1f / displacementVector.X;
            var intersectionX1 = (rect.Left - line.Start.X) * oodX;
            var intersectionX2 = (rect.Right - line.Start.X) * oodX;
            if(intersectionX1 > intersectionX2)
            {
                var temp = intersectionX1;
                intersectionX1 = intersectionX2;
                intersectionX2 = temp;
            }

            if (intersectionX1 > fractionMin)
                fractionMin = intersectionX1;
            if (intersectionX2 < fractionMax)
                fractionMax = intersectionX2;

            if (intersectionX1 > intersectionX2)
                return intersectionobject;
            
            var oodY = 1f / displacementVector.Y;
            var intersectionY1 = (rect.Top - line.Start.Y) * oodY;
            var intersectionY2 = (rect.Bottom - line.Start.Y) * oodY;
            if(intersectionY1 > intersectionY2)
            {
                var temp = intersectionY1;
                intersectionY1 = intersectionY2;
                intersectionY2 = temp;
            }

            if (intersectionY1 > fractionMin)
                fractionMin = intersectionY1;
            if (intersectionY2 < fractionMax)
                fractionMax = intersectionY2;

            if (intersectionY1 > intersectionY2)
                return intersectionobject;

            intersectionobject.Fraction = fractionMin;
            if(fractionMin <= 1)
                intersectionobject.Intersects = true;

            intersectionobject.IntersectionPoint = new Point(
                line.Start.X + displacementVector.X * fractionMin, 
                line.Start.Y + displacementVector.Y * fractionMin);

            return intersectionobject;
        }

        public static IntersectionObject LinesegmentToCircleIntersection(LineSegment line, Circle circle)
        {
            var intObject = new IntersectionObject();
            intObject.Intersects = false;

            Vector lineAsNormVec = new Vector(line.End.X - line.Start.X, line.End.Y - line.Start.Y).Normalize();
            Vector vec = new Vector(line.Start - circle.Center);
            float b = MathHelpers.DotProduct(vec, lineAsNormVec);
            float c = MathHelpers.DotProduct(vec, vec) - circle.Radius * circle.Radius;

            // No intersection
            if (c > 0f && b > 0f)
                return intObject;

            // Ray going away from circle
            float disc = b * b - c;
            if (disc < 0f)
                return intObject;

            float t = -b - (float)Math.Sqrt(disc);
            // Intersection is further away then the length of the linesegment
            if (t > line.Length)
                return intObject;
            if (t < 0f)
                t = 0f;

            Point intPoint = new Point(line.Start.X + t * lineAsNormVec.X, line.Start.Y + t * lineAsNormVec.Y);
            intObject.Intersects = true;
            intObject.IntersectionPoint = intPoint;
            intObject.Fraction = Distance(intPoint, line.Start) / line.Length;
            return intObject;
        }

        private static float Distance(Point pt1, Point pt2)
        {
            return (float)Math.Sqrt((pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) + (pt2.X - pt1.X) * (pt2.X - pt1.X));
        }

    }
}
