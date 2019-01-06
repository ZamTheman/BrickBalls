using col = Collision2D.BasicGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Collision2D.BoundingShapes;
using BallBreaker.Helpers;

namespace BallBreaker.Sprites
{
    public abstract class SpriteBase
    {
        public Texture2D Image { get; set; }
        public col.Vector Size { get; set; }
        public Aabb BoundingBox { get; set; }
        public Circle BoundingCircle { get; set; }

        private Converters converters;

        public SpriteBase()
        {
            converters = new Converters();
        }
        
        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                Image, 
                converters.ConvertAabbToXnaRectangle(BoundingBox), 
                Color.White);
        }
    }
}
