using Collision2D.BasicGeometry;
using NUnit.Framework;

namespace Collision2DTests
{
    [TestFixture]
    public class BasicGeometryTests
    {
        [Test]
        public void HorizontalLineSegmentIntersectsWithLineSegmentTest()
        {
            var horizontalLine = new LineSegment(new Point(5, 5), new Point(10, 5));
            var line45degrees = new LineSegment(new Point(5, 2), new Point(10, 7));
            var lineNeg45degrees = new LineSegment(new Point(5, 7), new Point(10, 2));
            var line135degrees = new LineSegment(new Point(10, 2), new Point(5, 7));
            var lineNeg135degrees = new LineSegment(new Point(10, 7), new Point(5, 2));

            var line45degreesResult = horizontalLine.IntersectWithLineSegment(line45degrees);
            var lineNeg45degreesResult = horizontalLine.IntersectWithLineSegment(lineNeg45degrees);
            var line135degreesResult = horizontalLine.IntersectWithLineSegment(line135degrees);
            var lineNeg135degreesResult = horizontalLine.IntersectWithLineSegment(lineNeg135degrees);

            Assert.AreEqual(line45degreesResult.IntersectionPoint.X, 8f);
            Assert.AreEqual(line45degreesResult.IntersectionPoint.Y, 5f);
            Assert.AreEqual(lineNeg45degreesResult.IntersectionPoint.X, 7f);
            Assert.AreEqual(lineNeg45degreesResult.IntersectionPoint.Y, 5f);
            Assert.AreEqual(line135degreesResult.IntersectionPoint.X, 7f);
            Assert.AreEqual(line135degreesResult.IntersectionPoint.Y, 5f);
            Assert.AreEqual(lineNeg135degreesResult.IntersectionPoint.X, 8f);
            Assert.AreEqual(lineNeg135degreesResult.IntersectionPoint.Y, 5f);
        }

        [Test]
        public void VerticalLineSegmentIntersectsWithLineSegmentTest()
        {
            var verticalLine = new LineSegment(new Point(3, 3), new Point(3, 7));
            var line20degrees = new LineSegment(new Point(2, 3), new Point(4, 4));
            var lineNeg20degrees = new LineSegment(new Point(4, 3), new Point(2, 4));
            var line45degrees = new LineSegment(new Point(2, 5), new Point(4, 7));
            var lineNeg45degrees = new LineSegment(new Point(4, 5), new Point(2, 7));

            var line20degreesResult = verticalLine.IntersectWithLineSegment(line20degrees);
            var lineNeg20degreesResult = verticalLine.IntersectWithLineSegment(lineNeg20degrees);
            var line45degreesResult = verticalLine.IntersectWithLineSegment(line45degrees);
            var lineNeg45degreesResult = verticalLine.IntersectWithLineSegment(lineNeg45degrees);

            Assert.AreEqual(line20degreesResult.IntersectionPoint.X, 3f);
            Assert.AreEqual(line20degreesResult.IntersectionPoint.Y, 3.5f);
            Assert.AreEqual(lineNeg20degreesResult.IntersectionPoint.X, 3f);
            Assert.AreEqual(lineNeg20degreesResult.IntersectionPoint.Y, 3.5f);
            Assert.AreEqual(line45degreesResult.IntersectionPoint.X, 3f);
            Assert.AreEqual(line45degreesResult.IntersectionPoint.Y, 6f);
            Assert.AreEqual(lineNeg45degreesResult.IntersectionPoint.X, 3f);
            Assert.AreEqual(lineNeg45degreesResult.IntersectionPoint.Y, 6f);
        }

    }
}
