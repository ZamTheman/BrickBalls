using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using Collision2D.HelperObjects;
using System;

namespace Collision2D.CollisionChecks
{
    public class DistanceChecks
    {
        public static IntersectionObject DistanceToClosestPointOnAbb(Point point, Aabb rect)
        {
            var returnObject = new IntersectionObject();

            var closestPoint = ClosestPointOnAabb(point, rect);
            var distanceToClosestPoint = Math.Abs(point.Distance(closestPoint));

            returnObject.ClosestPoint = closestPoint;
            returnObject.Distance = distanceToClosestPoint;

            return returnObject;
        } 

        private static Point ClosestPointOnAabb(Point point, Aabb rect)
        {
            Point closestPoint = new Point();
            float x = point.X;
            float y = point.Y;

            if (x < rect.Left) x = rect.Left; // v = max(v, b.min[i])
            if (x > rect.Right) x = rect.Right; // v = min(v, b.max[i])
            closestPoint.X = x;

            if (y < rect.Top) y = rect.Top;
            if (y > rect.Bottom) y = rect.Bottom;
            closestPoint.Y = y;

            return closestPoint;
        }

    }
}
