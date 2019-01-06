using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using Collision2D.CollisionChecks;
using NUnit.Framework;

namespace Collision2DTests.CollisionChectsTests
{
    [TestFixture]
    public class IntersectionTestsTests
    {
        private Aabb rect = new Aabb(4, 2, 10, 10);
        private Circle circle = new Circle(new Point(5, 5), 2);
        private const float TOLERANCE = 0.1f;

        [Test]
        [TestCase(2, 9, 7, 14, true, 4, 11)]
        [TestCase(7, 14, 2, 9, true, 5, 12)]
        [TestCase(14.1f, 3, 14.1f, 5, false, 0, 0)]
        [TestCase(14.1f, 5, 14.1f, 3, false, 0, 0)]
        [TestCase(2, 4, 6, 0, true, 4, 2)]
        [TestCase(6, 0, 2, 4, true, 4, 2)]
        [TestCase(0, 5, 1, 5, false, 0, 0)]
        public void LinesegmentToAabbIntersectionTest1_1(float x1, float y1, float x2, float y2, bool intersects, float resX, float resY)
        {
            LineSegment line = new LineSegment(new Point(x1, y1), new Point(x2, y2));

            var result = IntersectionTests.LinesegmentToAabbIntersection(line, rect);

            Assert.NotNull(result);
            Assert.AreEqual(result.Intersects, intersects);
            if (!intersects) return;
            Assert.AreEqual(result.IntersectionPoint.X, resX);
            Assert.AreEqual(result.IntersectionPoint.Y, resY);
        }

        [Test]
        [TestCase(0, 5, 10, 5, true, 3, 5)]
        [TestCase(10, 5, 5, 5, true, 7, 5)]
        [TestCase(2, 3, 5, 4, true, 3.63f, 3.543f)]
        [TestCase(5, 2, 6, 4, true, 5.537f, 3.073f)]
        [TestCase(8, 6, 6, 6, true, 6.732f, 6f)]
        [TestCase(3.1f, 4, 3.1f, 6, true, 3.1f, 4.376f)]
        [TestCase(3.1f, 6, 3.1f, 4, true, 3.1f, 5.624f)]
        public void LinesegmentToCircleIntersectionTest(float x1, float y1, float x2, float y2, bool intersects, float resX, float resY)
        {
            LineSegment line = new LineSegment(new Point(x1, y1), new Point(x2, y2));

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
