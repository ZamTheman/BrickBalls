using Collision2D.BoundingShapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using col = Collision2D.BasicGeometry;

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
        
        public col.Vector Update(col.Point ballPosition)
        {
            var state = Mouse.GetState();
            var returnVector = new col.Vector(0, 0);
            if(state.LeftButton == ButtonState.Pressed &&
                oldState != null &&
                oldState.LeftButton != ButtonState.Pressed)
            {
                oldState = state;
                var direction = new col.Vector(
                    state.Position.X - ballPosition.X,
                    state.Position.Y - ballPosition.Y);
                
                return direction.Normalize();
            }
            else
            {
                oldState = state;

                return new col.Vector(0, 0);
            }
        } 
    }
}
