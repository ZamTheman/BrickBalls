using Microsoft.Xna.Framework.Input;

namespace BallBreaker.Managers
{
    public class InputManager
    {
        private MouseState oldState; 

        public MouseState GetMouseState()
        {
            return Mouse.GetState();
        }

        public KeyboardState GetKeyboardState()
        {
            return Keyboard.GetState();
        }
    }
}
