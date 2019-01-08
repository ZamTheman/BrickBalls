using BallBreaker.Screens;
using BallBreaker.Sprites;
using System.Collections.Generic;
using Collision2D.BasicGeometry;
using Collision2D.HelperObjects;
using BallBreaker.HelperObjects;
using static BallBreaker.HelperObjects.Enums;

namespace BallBreaker.Helpers
{
    public static class CollisionChecks
    {
        // Walls offsetted one ball radius to be checked with circle center
        private static List<Sprites.Wall> wallSegments;

        public static CollisionObject WallCollision(Ball ball, List<List<Brick>> gameMatrix, Dictionary<Walls, ScreenBorder> borders)
        {
            var firstIntersection = float.MaxValue;
            CollisionObject intersection = new CollisionObject();
            intersection.IntersectionObject = new IntersectionObject();
            intersection.IntersectionObject.Intersects = false;

            foreach (var wall in wallSegments)
            {
                if (wall.HaveCollidedThisTurn)
                    continue;

                var movementStepAsLine = new LineSegment(ball.BoundingCircle.Center, ball.BoundingCircle.Center + ball.MovementStepVector);
                var wallIntersection = movementStepAsLine.IntersectWithLineSegment(wall.lineSegment);
                if (wallIntersection.Intersects && wallIntersection.Fraction < firstIntersection)
                {
                    intersection = new CollisionObject();
                    intersection.IntersectionObject = wallIntersection;
                    intersection.IntersectionObject.IntersectionType = IntersectionType.Wall;
                    intersection.IntersectionObject.Intersects = true;
                    if (intersection.IntersectionObject.IntersectionPoint.X == wall.lineSegment.Start.X)
                        intersection.Wall = HelperObjects.Wall.Left;
                    else if (intersection.IntersectionObject.IntersectionPoint.X == wall.lineSegment.Start.X)
                        intersection.Wall = HelperObjects.Wall.Right;
                    else
                        intersection.Wall = HelperObjects.Wall.Top;

                    wall.HaveCollidedThisTurn = true;
                }
            }

            return intersection;
        }

        public static void GenerateWallSegments(float offset, List<List<Brick>> gameMatrix, Dictionary<Walls, ScreenBorder> borders)
        {
            var walls = new List<Sprites.Wall>();
            var leftBorder = borders[Walls.Left].BoundingBox.Right + offset;
            var rightBorder = borders[Walls.Right].BoundingBox.Left - offset;
            var topBorder = borders[Walls.Top].BoundingBox.Bottom + offset;

            // Top border
            walls.Add(new Sprites.Wall() { lineSegment = new LineSegment(leftBorder, topBorder, rightBorder, topBorder) });

            var endY = topBorder + 64 - offset;
            if (gameMatrix[0][0] != null)
                endY -= offset;

            walls.Add(new Sprites.Wall() { lineSegment = new LineSegment(leftBorder, topBorder, leftBorder, endY) });

            endY = topBorder + 64 - offset;
            if (gameMatrix[0][7] != null)
                endY -= offset;

            walls.Add(new Sprites.Wall() { lineSegment = new LineSegment(rightBorder, topBorder, rightBorder, endY) });

            float startY = topBorder + 64 - offset;
            for (int i = 0; i < gameMatrix.Count; i++)
            {
                var leftWall = new Sprites.Wall() { lineSegment = new LineSegment(leftBorder, startY + i * 64, leftBorder, startY + i * 64 + 64) };
                var rightWall = new Sprites.Wall() { lineSegment = new LineSegment(rightBorder, startY + i * 64, rightBorder, startY + i * 64 + 64) };

                if (gameMatrix[i] != null)
                {
                    if (gameMatrix[i][0] != null)
                        leftWall = null;
                    if (gameMatrix[i][7] != null)
                        rightWall = null;
                }

                if (i != 0)
                {
                    if (leftWall != null && gameMatrix[i - 1] != null)
                    {
                        if (gameMatrix[i - 1][0] != null)
                        {
                            leftWall.lineSegment.Start.Y += offset;
                        }
                    }
                    if (rightWall != null && gameMatrix[i - 1] != null)
                    {
                        if (gameMatrix[i - 1][7] != null)
                        {
                            rightWall.lineSegment.Start.Y += offset;
                        }
                    }
                }
                    
                if (i != gameMatrix.Count - 1)
                {
                    if (leftWall != null && gameMatrix[i + 1] != null)
                    {
                        if (gameMatrix[i + 1][0] != null)
                        {
                            leftWall.lineSegment.End.Y -= offset;
                        }
                    }
                    if (rightWall != null && gameMatrix[i + 1] != null)
                    {
                        if (gameMatrix[i + 1][7] != null)
                        {
                            rightWall.lineSegment.End.Y -= offset;
                        }
                    }
                }

                if (leftWall != null)
                    walls.Add(leftWall);

                if (rightWall != null)
                    walls.Add(rightWall);

            }

            walls.Add(new Sprites.Wall() { lineSegment = new LineSegment(leftBorder, 712, leftBorder, 800) });
            walls.Add(new Sprites.Wall() { lineSegment = new LineSegment(rightBorder, 712, rightBorder, 800) });

            wallSegments = walls;
        }

        public static void ResolveWallCollission(Ball ball, CollisionObject collisionObject)
        {
            if (collisionObject.Wall == HelperObjects.Wall.Left || collisionObject.Wall == HelperObjects.Wall.Right)
                ball.Velocity.X *= -1;
            if (collisionObject.Wall == HelperObjects.Wall.Top)
                ball.Velocity.Y *= -1;

            SetMovementStep(ball, collisionObject.IntersectionObject);
        }
        
        public static CollisionObject BrickCollision(Ball ball, Brick brick, bool[,] brickMatrix)
        {
            var vectorAsLine = new LineSegment(ball.BoundingCircle.Center, ball.BoundingCircle.Center + ball.MovementStepVector);

            var minowskiGeometry = new MinowskiSumOfAabbAndCircle(brick.BoundingBox, ball.BoundingCircle, brickMatrix);
            var collisionObject = new CollisionObject();
            collisionObject.IntersectionObject = minowskiGeometry.Intersection(vectorAsLine);
            collisionObject.Brick = brick;

            if (collisionObject.IntersectionObject.Intersects && collisionObject.IntersectionObject.IntersectionType != IntersectionType.Circle)
            {
                if (collisionObject.IntersectionObject.IntersectionPoint.X == brick.BoundingBox.Left - ball.BoundingCircle.Radius ||
                    collisionObject.IntersectionObject.IntersectionPoint.X == brick.BoundingBox.Right + ball.BoundingCircle.Radius)
                    collisionObject.IntersectionObject.IntersectionType = IntersectionType.VerticalLine;
                else
                    collisionObject.IntersectionObject.IntersectionType = IntersectionType.HorizontalLine;
            }

            return collisionObject;
        }

        public static void ResolveDualCollission(Ball ball, CollisionObject collisionObject)
        {
            ball.Velocity.Y *= -1;
            ball.Velocity.X *= -1;

            SetMovementStep(ball, collisionObject.IntersectionObject);
        }

        public static void ResolveBrickCollission(Ball ball, CollisionObject collisionObject)
        {
            if (collisionObject.IntersectionObject.IntersectionType == IntersectionType.Circle)
                ResolveCircleCollission(ball, collisionObject);
            else
                ResolveLineCollision(ball, collisionObject);
        }

        public static void ResetOffsetWallsCollisionStatus()
        {
            foreach (var wall in wallSegments)
            {
                wall.HaveCollidedThisTurn = false;
            }
        }
        
        private static void ResolveLineCollision(Ball ball, CollisionObject collisionObject)
        {
            if (collisionObject.IntersectionObject.IntersectionType == IntersectionType.HorizontalLine)
                ball.Velocity.Y *= -1;
            if (collisionObject.IntersectionObject.IntersectionType == IntersectionType.VerticalLine)
                ball.Velocity.X *= -1;

            SetMovementStep(ball, collisionObject.IntersectionObject);
        }

        private static void ResolveCircleCollission(Ball ball, CollisionObject collisionObject)
        {
            var closestPoint = collisionObject.Brick.BoundingBox.GetClosestCorner(collisionObject.IntersectionObject.IntersectionPoint);
            var intPoint = collisionObject.IntersectionObject.IntersectionPoint;
            Vector ballToIntPoint = new Vector(
                closestPoint.X - intPoint.X,
                closestPoint.Y - intPoint.Y);
            Vector intPlaneAsVector = new Vector(ballToIntPoint.Y, -ballToIntPoint.X);
            float adjustedIntPlaneAngle = MathHelpers.FixAngleToPositiveWithinOneUnitCircle(intPlaneAsVector.Angle);
            float adjustedVelocityAngle = MathHelpers.FixAngleToPositiveWithinOneUnitCircle(ball.Velocity.Angle);
            float diffAngle = adjustedIntPlaneAngle - adjustedVelocityAngle;
            diffAngle = MathHelpers.FixAngleToPositiveWithinOneUnitCircle(diffAngle);
            float outAngle = adjustedIntPlaneAngle + diffAngle;
            outAngle = MathHelpers.FixAngleToPositiveWithinOneUnitCircle(outAngle);
            ball.Velocity.Angle = outAngle;

            SetMovementStep(ball, collisionObject.IntersectionObject);
        }

        private static void SetMovementStep(Ball ball, IntersectionObject intObject)
        {
            var distanceBallStartToIntersection = ball.Position.Distance(intObject.IntersectionPoint);
            if (float.IsNaN(distanceBallStartToIntersection))
            {
                ball.MovementStep = 0.01f;
            }
            else
            {
                var fractionOfMovementVector = distanceBallStartToIntersection / ball.MovementStepVector.Length;
                ball.MovementStep -= ball.MovementStep * fractionOfMovementVector;
            }
            ball.Position = intObject.IntersectionPoint;
        }
    }
}
