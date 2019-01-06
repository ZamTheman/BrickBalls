using col = Collision2D.BasicGeometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BallBreaker.Sprites
{
    public class DebugLine: SpriteBase
    {
        public col.LineSegment Line;
                
        public DebugLine()
        {
            Line = new col.LineSegment(new col.Point(0,0), new col.Point(0,0));
        }

        public DebugLine(col.LineSegment line)
        {
            Line = line;
        }

        public void Load(ContentManager content)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        
        }
    }
}
