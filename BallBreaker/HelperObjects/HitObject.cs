using Collision2D.BasicGeometry;

namespace BallBreaker.HelperObjects
{
    public class HitObject
    {
        public int ColliderId;
        public Point Position;
        public Vector Delta;
        public Vector Normal;
        public float Time;
        
        public HitObject(int colliderId)
        {
            ColliderId = colliderId;
            Position = new Point();
            Delta = Vector.Zero;
            Normal = Vector.Zero;
            Time = 0;
        }
    }
}
