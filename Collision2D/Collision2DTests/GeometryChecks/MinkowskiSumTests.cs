using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using Collision2D.HelperObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Collision2DTests.GeometryChecks
{
    [TestFixture]
    public class MinkowskiSumTests
    {
        [Test]
        [TestCase(false, false, false,
                    false, false, false,
                    false, false, false,
                    64f, 64f, 64f, 64f,
                    typeof(Circle), typeof(Circle),
                    typeof(Circle), typeof(Circle))]
        [TestCase(true, false, false,
                    false, false, false,
                    false, false, false,
                    64f, 64f, 64f, 64f,
                    null, typeof(Circle),
                    typeof(Circle), typeof(Circle))]
        [TestCase(false, true, false,
                    false, false, false,
                    false, false, false,
                    64f, 64f, 64f, 64f,
                    null, null,
                    typeof(Circle), typeof(Circle))]
        [TestCase(false, false, true,
                    false, false, false,
                    false, false, false,
                    64f, 64f, 64f, 64f,
                    typeof(Circle), null,
                    typeof(Circle), typeof(Circle))]
        public void TestMinkowskiSum(bool _11, bool _12, bool _13, bool _21, bool _22, bool _23, bool _31, bool _32, bool _33, float leftLength, float rightLength, float topLength, float bottomLength, Type topLeft, Type topRight, Type bottomRight, Type bottomLeft)
        {
            var boolMatrix = CreateBoolMatrix(_11, _12, _13, _21, _22, _23, _31, _32, _33);

            var circle = new Circle(new Point(0, 0), 10);
            var aabb = new Aabb(0, 0, 64, 64);

            var minSum = new MinowskiSumOfAabbAndCircle(aabb, circle, boolMatrix);

            Assert.AreEqual(leftLength, minSum.Left.Length);
            Assert.AreEqual(rightLength, minSum.Right.Length);
            Assert.AreEqual(topLength, minSum.Top.Length);
            Assert.AreEqual(bottomLength, minSum.Bottom.Length);
            if (topLeft != null)
                Assert.AreEqual(topLeft, minSum.TopLeftCorner.GetType());
            else
                Assert.IsNull(minSum.TopLeftCorner);
            if (topRight != null)
                Assert.AreEqual(topRight, minSum.TopRightCorner.GetType());
            else
                Assert.IsNull(minSum.TopRightCorner);
            if (bottomRight != null)
                Assert.AreEqual(bottomRight, minSum.BottomRightCorner.GetType());
            else
                Assert.IsNull(minSum.BottomRightCorner);
            if (bottomLeft != null)
                Assert.AreEqual(bottomLeft, minSum.BottomLeftCorner.GetType());
            else
                Assert.IsNull(minSum.BottomLeftCorner);
        }

        private static bool[,] CreateBoolMatrix(bool _11, bool _12, bool _13, bool _21, bool _22, bool _23, bool _31, bool _32, bool _33)
        {
            return new bool[3, 3] { 
                { _11, _21, _31 },
                { _12, _22, _32 },
                { _13, _23, _33 }
            };
        }
    }
}
