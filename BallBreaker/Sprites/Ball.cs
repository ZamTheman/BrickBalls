using col = Collision2D.BasicGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BallBreaker.Sprites
{
    public class Ball: SpriteBase
    {
        public float Speed = 30; // Pixels per second
        public col.Vector Velocity { get; set; }
        public col.Vector MovementStepVector
        {
            get { return Velocity * MovementStep; }
        }
        public col.Point Position
        {
            get { return BoundingCircle.Center; }
            set { BoundingCircle.Center = value; }
        }
        public col.Point NewPosition { get; set; }
        public float MovementStep { get; set; }

        public Ball()
        {
            Velocity = col.Vector.Zero;
        }

        public override void Update(GameTime gameTime)
        {
            Position += MovementStepVector;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Image,
                new Vector2(
                    BoundingCircle.Center.X - BoundingCircle.Radius,
                    BoundingCircle.Center.Y - BoundingCircle.Radius),
                Color.White);
        }
    }
}
