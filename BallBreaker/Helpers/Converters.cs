using Microsoft.Xna.Framework;
using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;

namespace BallBreaker.Helpers
{
    public class Converters
    {
        public Rectangle ConvertAabbToXnaRectangle(Aabb rect)
        {
            return new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        }

        public Vector2 ConvertVectorToXnaVector2(Vector vec)
        {
            return new Vector2(vec.X, vec.Y);
        }
    }
}
