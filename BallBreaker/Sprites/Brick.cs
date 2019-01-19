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

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            spriteBatch.Draw(pixel, new Rectangle((int)BoundingBox.Left + 1, (int)BoundingBox.Top + 1, (int)BoundingBox.Height - 2, (int)BoundingBox.Width - 2), GetColor());
            spriteBatch.Draw(Image, new Rectangle((int)BoundingBox.Left, (int)BoundingBox.Top, (int)BoundingBox.Height, (int)BoundingBox.Width), Color.White);

            var stringSize = Font.MeasureString(Counter.ToString());
            spriteBatch.DrawString(
                Font, 
                Counter.ToString(), 
                new Vector2(
                    (int)(BoundingBox.Center.X - stringSize.X * 0.5f), 
                    (int)(BoundingBox.Center.Y - stringSize.Y * 0.5f + 2)), 
                Color.White);
        }

        private Color GetColor()
        {
            int localCounter = Counter;
            while (true)
            {
                if (localCounter < 170)
                    break;

                localCounter -= 170;
            }

            if (localCounter < 10)
                return new Color(27, 50, 95);
            else if (localCounter < 20)
                return new Color(95, 87, 147);
            else if (localCounter < 30)
                return new Color(230, 30, 44);
            else if (localCounter < 40)
                return new Color(37, 101, 83);
            else if (localCounter < 50)
                return new Color(245, 135, 35);
            else if (localCounter < 60)
                return new Color(50, 124, 203);
            else if (localCounter < 70)
                return new Color(226, 117, 148);
            else if (localCounter < 80)
                return new Color(131, 163, 0);
            else if (localCounter < 90)
                return new Color(139, 132, 183);
            else if (localCounter < 100)
                return new Color(219, 97, 128);
            else if (localCounter < 110)
                return new Color(39, 78, 69);
            else if (localCounter < 120)
                return new Color(58, 45, 25);
            else if (localCounter < 130)
                return new Color(74, 151, 10);
            else if (localCounter < 140)
                return new Color(170, 31, 67);
            else if (localCounter < 150)
                return new Color(43, 78, 114);
            else if (localCounter < 160)
                return new Color(232, 55, 62);
            else
                return new Color(117, 207, 182);
        }
    }
}
