using BallBreaker.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
