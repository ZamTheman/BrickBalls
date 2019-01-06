using System;

namespace Collision2D.BasicGeometry
{
    public class Vector: Point
    {
        public static Vector Zero { get => new Vector(0,0); }
        public float Length
        {
            get
            {
                float x = Math.Abs(X);
                float y = Math.Abs(Y);
                return (float)Math.Sqrt(x * x + y * y);
            }
            set
            {
                var vec = this.Normalize();
                this.X = vec.X * value;
                this.Y = vec.Y * value;
            }
        }
        public float Angle
        {
            get => (float)Math.Atan2(Y, X);
            set
            {
                var angle = AdjustAngleToPositiveNegativePi(value);
                var oldLength = Length;
                X = (float)Math.Cos(angle) * oldLength;
                Y = (float)Math.Sin(angle) * oldLength;
            }
        }

        public Vector(float x, float y): base (x,y) { }
        public Vector(Point pt): this(pt.X, pt.Y) { }
        public Vector(Point origin, Point pt2) : this(pt2.X - origin.X, pt2.Y - origin.Y) { }

        public float Direction()
        {
            return (float)Math.Atan2(Y, X);
        }
        
        public void Rotate(float angle)
        {
            if (Length == 0f)
                return;

            var ca = (float)Math.Cos(angle);
            var sa = (float)Math.Sin(angle);
            X = ca * X - sa * Y;
            Y = sa * X + ca * Y;
        }
        
        public Vector Normalize()
        {
            var localLength = Length == 0 ? 0.01f : Length;
            return new Vector(X / localLength, Y / localLength);
        }

        public static Vector operator *(Vector vec, float value)
        {
            return new Vector(vec.X * value, vec.Y * value);
        }

        public static Vector operator +(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X + vec2.X, vec1.Y + vec2.Y);
        }

        public static Vector operator -(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X - vec2.X, vec1.Y - vec2.Y);
        }

        public static Vector AddPoint(Vector vec, Point pt)
        {
            return new Vector(vec.X + pt.X, vec.Y + pt.Y);
        }

        public static Vector SubtractPoint(Vector vec, Point pt)
        {
            return new Vector(vec.X - pt.X, vec.Y - pt.Y);
        }

        private static float AdjustAngleToPositiveNegativePi(float angle)
        {
            while (true)
            {
                if (angle < -Math.PI)
                    angle += (float)Math.PI * 2;
                else if (angle > Math.PI)
                    angle -= (float)Math.PI * 2;
                else
                    return angle;
            }
        }
    }
}
