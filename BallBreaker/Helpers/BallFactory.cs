using BallBreaker.Sprites;
using col = Collision2D.BasicGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BallBreaker.Helpers
{
    public class BallFactory : SpriteBase
    {
        public float BallRadius { get; set; }
        public int BallsInFactory { get; set; } = 1;
        public double TimeSinceLastBall { get; set; } = 0;
        public col.Vector InitialVelocity { get; set; }
        public col.Point Position { get; set; }
        public bool IsFirstBall { get; set; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Image,
                new Vector2(
                    Position.X - BallRadius,
                    Position.Y - BallRadius),
                Color.White);
        }
    }
}
