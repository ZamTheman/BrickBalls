using Collision2D.HelperObjects;
using System;

namespace Collision2D.BasicGeometry
{
    public class LineSegment
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public Vector Vector { get => new Vector(End.X - Start.X, End.Y - Start.Y); }
        private float sloap;
        private float yIntercept;
        private bool isVertical = false;
        private bool isHorizontal = false;

        public float Length { get => Vector.Length; }
        public float Angle { get => Vector.Direction(); }

        public LineSegment(float x1, float y1, float x2, float y2)
        {
            Start = new Point(x1, y1);
            End = new Point(x2, y2);

            SetupLineSegment();
        }

        public LineSegment(Point start, Point end)
        {
            Start = start;
            End = end;

            SetupLineSegment();
        }

        private void SetupLineSegment()
        {
            if (End.X == Start.X)
                isVertical = true;
            else if (End.Y == Start.Y)
                isHorizontal = true;

            if (isVertical || isHorizontal)
                return;

            sloap = (End.Y - Start.Y) / (End.X - Start.X);
            yIntercept = Start.Y - sloap * Start.X;
        }

        public void RotateLineAroundStart(float deltaAngle)
        {
            var ca = (float)Math.Cos(deltaAngle);
            var sa = (float)Math.Sin(deltaAngle);
            var pointX = End.X - Start.X;
            var pointY = End.Y - Start.Y;
            var rotatedPointX = ca * pointX - sa * pointY;
            var rotatedPointY = sa * pointX + ca * pointY;
            End.X = rotatedPointX + Start.X;
            End.Y = rotatedPointY + Start.Y;
        }

        public IntersectionObject IntersectWithLineSegment(LineSegment line)
        {
            var intersectionObject = new IntersectionObject();
            intersectionObject.Intersects = false;
            var intersectionPoint = new Point(0, 0);

            // Parallell lines, no intersections
            if (isVertical && line.isVertical || isHorizontal && line.isHorizontal)
                return intersectionObject;

            if ((line.isHorizontal && isVertical) || (line.isVertical && isHorizontal))
            {
                var verticalLine = line.isVertical ? line : this;
                var horizontalLine = line.isHorizontal ? line : this;
                intersectionPoint = new Point(verticalLine.Start.X, horizontalLine.Start.Y);

                if (!IsIntersecting(verticalLine, horizontalLine, intersectionPoint))
                    return intersectionObject;
            }
            else if (line.isHorizontal || isHorizontal)
            {
                var horizontalLine = line.isHorizontal ? line : this;
                var generalLine = line.isHorizontal ? this : line;

                var yIntersection = horizontalLine.Start.Y;
                var xIntersection = (yIntersection - generalLine.yIntercept) / generalLine.sloap;
                intersectionPoint = new Point(xIntersection, yIntersection);

                if (!IsIntersecting(generalLine, horizontalLine, intersectionPoint))
                    return intersectionObject;
            }
            else if (line.isVertical || isVertical)
            {
                var verticalLine = line.isVertical ? line : this;
                var generalLine = line.isVertical ? this : line;

                var xIntersection = verticalLine.Start.X;
                var yIntersection = generalLine.sloap * xIntersection + generalLine.yIntercept;
                intersectionPoint = new Point(xIntersection, yIntersection);

                if (!IsIntersecting(generalLine, verticalLine, intersectionPoint))
                    return intersectionObject;
            }
            else if (sloap == line.sloap)
            {
                return intersectionObject;
            }
            else
            {
                var xIntersection = (line.yIntercept - yIntercept) / (sloap - line.sloap);
                var yIntersection = line.sloap * xIntersection + line.yIntercept;
                intersectionPoint = new Point(xIntersection, yIntersection);

                if (!IsIntersecting(line, this, intersectionPoint))
                    return intersectionObject;
            }

            intersectionObject.IntersectionPoint = intersectionPoint;
            intersectionObject.Intersects = true;
            intersectionObject.Fraction = intersectionObject.IntersectionPoint.Distance(line.Start) / line.Length;
            return intersectionObject;
        }

        private bool IsIntersecting(LineSegment line1, LineSegment line2, Point intersectionPoint)
        {
            float line1MinX = Math.Min(line1.Start.X, line1.End.X);
            float line1MaxX = Math.Max(line1.Start.X, line1.End.X);
            float line1MinY = Math.Min(line1.Start.Y, line1.End.Y);
            float line1MaxY = Math.Max(line1.Start.Y, line1.End.Y);

            float line2MinX = Math.Min(line2.Start.X, line2.End.X);
            float line2MaxX = Math.Max(line2.Start.X, line2.End.X);
            float line2MinY = Math.Min(line2.Start.Y, line2.End.Y);
            float line2MaxY = Math.Max(line2.Start.Y, line2.End.Y);

            // Intersection point is outside line segments
            if (intersectionPoint.X > line1MaxX || intersectionPoint.X > line2MaxX ||
                intersectionPoint.X < line1MinX || intersectionPoint.X < line2MinX ||
                intersectionPoint.Y > line1MaxY || intersectionPoint.Y > line2MaxY ||
                intersectionPoint.Y < line1MinY || intersectionPoint.Y < line2MinY)
                return false;

            return true;
        }

        private IntersectionObject FindIntersectionPoint(
            IntersectionObject intersectionObject,
            LineSegment line,
            float xIntersect,
            float yIntersect)
        {

            float line1MinX = Math.Min(line.Start.X, line.End.X);
            float line1MaxX = Math.Max(line.Start.X, line.End.X);
            float line1MinY = Math.Min(line.Start.Y, line.End.Y);
            float line1MaxY = Math.Max(line.Start.Y, line.End.Y);

            float line2MinX = Math.Min(Start.X, End.X);
            float line2MaxX = Math.Max(Start.X, End.X);
            float line2MinY = Math.Min(Start.Y, End.Y);
            float line2MaxY = Math.Max(Start.Y, End.Y);

            // Intersection point is outside line segments
            if (xIntersect > line1MaxX || xIntersect > line2MaxX ||
                xIntersect < line1MinX || xIntersect < line2MinX ||
                yIntersect > line1MaxY || yIntersect > line2MaxY ||
                yIntersect < line1MinY || yIntersect < line2MinY)
                return intersectionObject;

            intersectionObject.IntersectionPoint = new Point(xIntersect, yIntersect);
            intersectionObject.Fraction = intersectionObject.IntersectionPoint.Distance(Start) / line.Length;
            intersectionObject.Intersects = true;

            return intersectionObject;
        }
    }
}
