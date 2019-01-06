using Collision2D.BasicGeometry;
using System;

namespace Collision2D.HelperObjects
{
    public static class MathHelpers
    {
        public static float DotProduct(Vector vec1, Vector vec2)
        {
            return vec1.X * vec2.X + vec1.Y * vec2.Y;
        }

        public static float FixAngleToPositiveWithinOneUnitCircle(float angle)
        {
            float adjustedAngle = angle;
            while (adjustedAngle < 0 || adjustedAngle > Math.PI * 2)
            {
                if (adjustedAngle < 0)
                    adjustedAngle += (float)Math.PI * 2;
                else if (adjustedAngle > Math.PI * 2)
                    adjustedAngle -= (float)Math.PI * 2;
                else
                    break;
            }

            return adjustedAngle;
        }
    }
}
