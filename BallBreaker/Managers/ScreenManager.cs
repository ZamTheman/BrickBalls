using BallBreaker.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BallBreaker.Managers
{
    public class ScreenManager
    {
        private IScreen currentScreen;
        private ContentManager content;
        
        public ScreenManager(ContentManager content, InputManager inputManager, Rectangle screenSize, int playAreaWidth, int playAreaHeight)
        {
            this.content = content;
            currentScreen = new GameScreen(content, inputManager, screenSize, playAreaWidth, playAreaHeight);
        }
                
        public IScreen GetCurrentScreen() => currentScreen;
    }
}
