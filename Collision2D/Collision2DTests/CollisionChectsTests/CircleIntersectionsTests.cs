using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using Collision2D.CollisionChecks;
using NUnit.Framework;

namespace Collision2DTests.CollisionChectsTests
{

    [TestFixture]
    public class CircleIntersectionsTests
    {
        private const float TOLERANCE = 0.1f;

        [TestCase(0, 0, 10, 10, true, 3.586f, 3.586f)]
        [TestCase(0, 5, 10, 5, true, 3f, 5f)]
        [TestCase(0, 3, 10, 3, true, 5f, 3f)]
        [TestCase(5, 10, 5, 0, true, 5f, 7f)]
        public void LineSegmentToCircleIntersectionTest(float x1, float y1, float x2, float y2, bool intersects, float resX, float resY)
        {
            LineSegment line = new LineSegment(new Point(x1, y1), new Point(x2, y2));
            Circle circle = new Circle(new Point(5, 5), 2);

            var result = IntersectionTests.LinesegmentToCircleIntersection(line, circle);

            Assert.NotNull(result);
            Assert.AreEqual(result.Intersects, intersects);
            if (!intersects) return;
            Assert.IsTrue(WithinTolerance(result.IntersectionPoint.X, resX));
            Assert.IsTrue(WithinTolerance(result.IntersectionPoint.Y, resY));
        }

        private bool WithinTolerance(float val, float refVal)
        {
            if (val > refVal - TOLERANCE && val < refVal + TOLERANCE)
                return true;

            return false;
        }
    }
}
