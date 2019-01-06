using Collision2D.BasicGeometry;

namespace BallBreaker.Sprites
{
    public class Wall
    {
        public LineSegment lineSegment { get; set; }
        public bool HaveCollidedThisTurn { get; set; } = false;
    }
}
