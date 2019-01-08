using Microsoft.Xna.Framework.Input;

namespace BallBreaker.Managers
{
    public class InputManager
    {
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
