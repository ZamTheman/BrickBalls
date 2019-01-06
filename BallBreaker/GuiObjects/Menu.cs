using BallBreaker.Managers;
using BallBreaker.Sprites;
using Collision2D.BoundingShapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallBreaker.GuiObjects
{
    public class Menu : SpriteBase
    {
        public Aabb aabb { get; private set; }

        private Dictionary<string, Delegate> menuItems = new Dictionary<string, Delegate>();
        private Vector2 centerTopPosition;
        private SpriteFont font;
        private InputManager inputManager;
        private MouseState oldMouseState;
        private string highlightedMenuItem;
        private float distanceBetween = 50;
        private Texture2D darkPixel;

        public Menu(
            Dictionary<string, Delegate> menuItems,
            Vector2 centerTopPosition,
            SpriteFont font,
            Texture2D darkPixel)
        {
            this.menuItems = menuItems;
            this.font = font;
            this.centerTopPosition = centerTopPosition;
            this.darkPixel = darkPixel;
            CreateBoundingBox();
            inputManager = new InputManager();
        }

        public override void Update(GameTime gameTime)
        {
            var mouseState = inputManager.GetMouseState();

            if (mouseState.X < aabb.Left ||
                mouseState.X > aabb.Right ||
                mouseState.Y < aabb.Top ||
                mouseState.Y > aabb.Bottom)
            {
                highlightedMenuItem = string.Empty;
            }
            else
            {
                float incrementingDistance = 0;
                bool noneHighlighted = true;
                foreach (var menuItem in menuItems)
                {
                    var menuItemStartY = centerTopPosition.Y + incrementingDistance;
                    var menuItemEndY = menuItemStartY + font.MeasureString(menuItem.Key).Y;

                    if (mouseState.Y > menuItemStartY &&
                        mouseState.Y < menuItemEndY)
                    {
                        highlightedMenuItem = menuItem.Key;
                        noneHighlighted = false;
                    }

                    if (mouseState.Y > menuItemStartY && 
                        mouseState.Y < menuItemEndY &&
                        oldMouseState.LeftButton == ButtonState.Pressed &&
                        mouseState.LeftButton == ButtonState.Released)
                    {
                        if (menuItem.Value != null)
                            menuItem.Value.DynamicInvoke();
                    }

                    incrementingDistance += distanceBetween;
                }

                if (noneHighlighted)
                    highlightedMenuItem = string.Empty;
            }

            oldMouseState = mouseState;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float incrementingDistance = 0;
            foreach (var menuItem in menuItems)
            {
                var stringWidth = font.MeasureString(menuItem.Key).X;
                var menuItemStartY = centerTopPosition.Y + incrementingDistance;
                var menuItemStartX = aabb.Center.X - stringWidth * 0.5f;

                if (menuItem.Key == highlightedMenuItem)
                {
                    spriteBatch.Draw(
                        darkPixel,
                        new Rectangle(
                                (int)aabb.Center.X - 100,
                                (int)menuItemStartY - 10,
                                200,
                                (int)font.MeasureString(menuItem.Key).Y + 10),
                        Color.White);
                }
                
                spriteBatch.DrawString(
                    font,
                    menuItem.Key,
                    new Vector2(menuItemStartX, menuItemStartY),
                    Color.White);

                incrementingDistance += distanceBetween;
            }
        }

        private void CreateBoundingBox()
        {
            float maxWidth = 0;
            float totalHeight = 0;
            
            var fontSize = Vector2.Zero;
            foreach (var menuItem in menuItems)
            {
                fontSize = font.MeasureString(menuItem.Key);
                if (fontSize.X > maxWidth)
                    maxWidth = fontSize.X;

                totalHeight += distanceBetween;
            }

            totalHeight += fontSize.Y;

            aabb = new Aabb(centerTopPosition.X - maxWidth * 0.5f, centerTopPosition.Y, maxWidth, totalHeight);
        }



    }
}
