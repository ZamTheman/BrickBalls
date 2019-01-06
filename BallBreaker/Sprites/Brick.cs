using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BallBreaker.Sprites
{
    public class Brick: SpriteBase
    {
        public int Counter { get; set; }
        public float MaxDistanceFromCenter { get => BoundingBox.DistanceCenterToCorner; }
        public SpriteFont Font { get; set; }
        public bool HaveCollidedThisTurn { get; set; } = false;
        public float OldYPos { get; set; }
        public float NewYPos { get { return OldYPos + 64; } }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            var stringSize = Font.MeasureString(Counter.ToString());
            spriteBatch.DrawString(
                Font, 
                Counter.ToString(), 
                new Vector2(
                    (int)(BoundingBox.Center.X - stringSize.X * 0.5f), 
                    (int)(BoundingBox.Center.Y - stringSize.Y * 0.5f + 2)), 
                Color.White);
        }
    }
}
