using Collision2D.BoundingShapes;
using Collision2D.BasicGeometry;
using Collision2D.CollisionChecks;
using System;
using System.Collections.Generic;

namespace Collision2D.HelperObjects
{
    public class MinowskiSumOfAabbAndCircle
    {
        public Aabb BaseRect { get; private set; }
        public Circle BaseCircle { get; private set; }

        public Aabb HorizontalRect { get; private set; }
        public Aabb VerticalRect { get; private set; }

        public Circle TopLeftCorner { get; private set; }
        public Circle TopRightCorner { get; private set; }
        public Circle BottomRightCorner { get; private set; }
        public Circle BottomLeftCorner { get; private set; }

        public LineSegment Top { get; private set; }
        public LineSegment Bottom { get; private set; }
        public LineSegment Left { get; private set; }
        public LineSegment Right { get; private set; }

        public MinowskiSumOfAabbAndCircle(Aabb rect, Circle circle, bool[,] matrix)
        {
            BaseRect = rect;

            bool circleTopLeft, circleTopRight, circleBottomRight, circleBottomLeft;
            circleTopLeft = circleTopRight = circleBottomRight = circleBottomLeft = false;
            if (!matrix[0, 1] && !matrix[0, 0] && !matrix[1, 0])
                circleTopLeft = true;
            if (!matrix[1, 0] && !matrix[2, 0] && !matrix[2, 1])
                circleTopRight = true;
            if (!matrix[2, 1] && !matrix[2, 2] && !matrix[1, 2])
                circleBottomRight = true;
            if (!matrix[1, 2] && !matrix[0, 2] && !matrix[0, 1])
                circleBottomLeft = true;
            
            TopLeftCorner = circleTopLeft ? new Circle(new Point(rect.Left, rect.Top), circle.Radius) : null;
            TopRightCorner = circleTopRight ? new Circle(new Point(rect.Right, rect.Top), circle.Radius) : null;
            BottomRightCorner = circleBottomRight ? new Circle(new Point(rect.Right, rect.Bottom), circle.Radius) : null;
            BottomLeftCorner = circleBottomLeft ? new Circle(new Point(rect.Left, rect.Bottom), circle.Radius) : null;

            Top = new LineSegment(new Point(rect.Left, rect.Top - circle.Radius),  new Point(rect.Right, rect.Top - circle.Radius));
            Right = new LineSegment(new Point(rect.Right + circle.Radius, rect.Top), new Point(rect.Right + circle.Radius, rect.Bottom));
            Bottom = new LineSegment(new Point(rect.Right, rect.Bottom + circle.Radius), new Point(rect.Left, rect.Bottom + circle.Radius));
            Left = new LineSegment(new Point(rect.Left - circle.Radius, rect.Bottom), new Point(rect.Left - circle.Radius, rect.Top));

            if (matrix[0, 0])
            {
                Top.Start.X += circle.Radius;
                Left.End.Y += circle.Radius;
            }
            if (matrix[2, 0])
            {
                Top.End.X -= circle.Radius;
                Right.Start.Y += circle.Radius;
            }
            if (matrix[0, 2])
            {
                Left.Start.Y -= circle.Radius;
                Bottom.End.X += circle.Radius;
            }
            if (matrix[2, 2])
            {
                Right.End.Y -= circle.Radius;
                Bottom.Start.X -= circle.Radius;
            }
        }
        
        public MinowskiSumOfAabbAndCircle(Aabb rect, Circle circle)
        {
            BaseRect = rect;
            BaseCircle = circle;

            HorizontalRect = new Aabb(BaseRect.Center, rect.Width + circle.Radius * 2, rect.Height);
            VerticalRect = new Aabb(BaseRect.Center, rect.Width, rect.Height + circle.Radius * 2);

            TopLeftCorner = new Circle(new Point(rect.Left, rect.Top), circle.Radius);
            TopRightCorner = new Circle(new Point(rect.Right, rect.Top), circle.Radius);
            BottomRightCorner = new Circle(new Point(rect.Right, rect.Bottom), circle.Radius);
            BottomLeftCorner = new Circle(new Point(rect.Left, rect.Bottom), circle.Radius);

            Top = new LineSegment(
                new Point(rect.Left, rect.Top - circle.Radius),
                new Point(rect.Right, rect.Top - circle.Radius));
            Bottom = new LineSegment(
                new Point(rect.Left, rect.Bottom + circle.Radius),
                new Point(rect.Right, rect.Bottom + circle.Radius));
            Left = new LineSegment(
                new Point(rect.Left - circle.Radius, rect.Top),
                new Point(rect.Left - circle.Radius, rect.Bottom));
            Right = new LineSegment(
                new Point(rect.Right + circle.Radius, rect.Top),
                new Point(rect.Right + circle.Radius, rect.Bottom));

        }

        public IntersectionObject Intersection(LineSegment line)
        {
            var intersectionObject = new IntersectionObject();
            intersectionObject.Fraction = float.MaxValue;
            intersectionObject.Intersects = false;

            var lineAsVector = line.Vector;

            var positiveAngleToCompare = MathHelpers.FixAngleToPositiveWithinOneUnitCircle(lineAsVector.Angle);

            List<IntersectionObject> lineObjects = new List<IntersectionObject>();
            List<IntersectionObject> circleObjects = new List<IntersectionObject>();
            if (positiveAngleToCompare > 0 && positiveAngleToCompare < Math.PI * 0.5)
            {
                lineObjects.Add(line.IntersectWithLineSegment(Top));
                lineObjects.Add(line.IntersectWithLineSegment(Left));
                if(TopLeftCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, TopLeftCorner));
                if (TopRightCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, TopRightCorner));
                if (BottomLeftCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, BottomLeftCorner));
            }
            else if (positiveAngleToCompare >= Math.PI * 0.5 && positiveAngleToCompare < Math.PI)
            {
                lineObjects.Add(line.IntersectWithLineSegment(Top));
                lineObjects.Add(line.IntersectWithLineSegment(Right));
                if (TopLeftCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, TopLeftCorner));
                if (TopRightCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, TopRightCorner));
                if (BottomRightCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, BottomRightCorner));
            }
            else if (positiveAngleToCompare >= Math.PI && positiveAngleToCompare < Math.PI * 1.5)
            {
                lineObjects.Add(line.IntersectWithLineSegment(Bottom));
                lineObjects.Add(line.IntersectWithLineSegment(Right));
                if (TopRightCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, TopRightCorner));
                if (BottomRightCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, BottomRightCorner));
                if (BottomLeftCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, BottomLeftCorner));
            }
            else
            {
                lineObjects.Add(line.IntersectWithLineSegment(Bottom));
                lineObjects.Add(line.IntersectWithLineSegment(Left));
                if (TopLeftCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, TopLeftCorner));
                if (BottomRightCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, BottomRightCorner));
                if (BottomLeftCorner != null)
                    circleObjects.Add(IntersectionTests.LinesegmentToCircleIntersection(line, BottomLeftCorner));
            }

            // If the hit is on a valid line, the circle should never be hit
            foreach(var lineObject in lineObjects)
            {
                if (lineObject.Intersects)
                {
                    if (lineObject.IntersectionPoint.Y == Top.Start.Y)
                        lineObject.IntersectionType = IntersectionType.HorizontalLine;
                    else
                        lineObject.IntersectionType = IntersectionType.VerticalLine;

                    lineObject.Aabb = BaseRect;
                    return lineObject;
                }
            }

            foreach (var circleObject in circleObjects)
            {
                if (circleObject.Intersects)
                {
                    circleObject.IntersectionType = IntersectionType.Circle;
                    circleObject.Aabb = BaseRect;
                    return circleObject;
                }
            }

            return intersectionObject;
        }
    }
}
