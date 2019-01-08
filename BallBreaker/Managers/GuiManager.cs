using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BallBreaker.GuiObjects;
using static BallBreaker.HelperObjects.Enums;

namespace BallBreaker.Managers
{
    public class GuiManager
    {
        private InputManager inputManager;

        private Texture2D whitePixel;
        private Texture2D grayPixel;
        private Texture2D darkGrayPixel;
        private Texture2D logo;
        private Texture2D grayCorner;
        private Texture2D darkGrayCorner;
        private Texture2D gameOverBackground;
        private Texture2D newGameBackground;

        private SpriteFont largeFont;
        private SpriteFont smallFont;

        private Menu menu;
        private List<GuiItem> guiItems;

        private Rectangle screenSize;
        private Dictionary<string, Delegate> menuItems;

        private bool drawCursor = false;
        private bool highLightOk = false;
        private double cursorTimer = 500;

        private KeyboardState oldKeyBoardState;
        private MouseState oldMouseState;

        public GuiManager(
            Rectangle screenSize,
            Dictionary<string, Delegate> menuItems,
            InputManager inputManager)
        {
            this.screenSize = screenSize;
            this.menuItems = menuItems;
            this.inputManager = inputManager;
            guiItems = new List<GuiItem>();
            oldKeyBoardState = inputManager.GetKeyboardState();
            oldMouseState = inputManager.GetMouseState();
        }

        public void LoadContent(ContentManager content)
        {
            gameOverBackground = content.Load<Texture2D>("Images/GameOverBackground");
            newGameBackground = content.Load<Texture2D>("Images/NewGameBackground");

            // Font
            largeFont = content.Load<SpriteFont>("GameFont");
            smallFont = content.Load<SpriteFont>("Fonts/GameFontSmall");

            // GUI Elements
            logo = content.Load<Texture2D>("Images/Logo");
            whitePixel = content.Load<Texture2D>("Images/WhitePixel");
            grayPixel = content.Load<Texture2D>("Images/GrayPixel");
            darkGrayPixel = content.Load<Texture2D>("Images/DarkGrayPixel");
            grayCorner = content.Load<Texture2D>("Images/GrayCorner");
            darkGrayCorner = content.Load<Texture2D>("Images/DarkGrayCorner");
            menu = new Menu(
                menuItems,
                new Vector2(screenSize.Right - 128, screenSize.Bottom - 170),
                largeFont,
                darkGrayPixel);

            SetupGuiItems();
        }

        public void Update(GameTime gameTime)
        {
            menu.Update(gameTime);
            if (GameState.State == State.NewGame)
            {
                UpdateNewGame(gameTime);
            }
        }

        private void UpdateNewGame(GameTime gameTime)
        {
            cursorTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (cursorTimer < 0)
            {
                drawCursor = !drawCursor;
                cursorTimer = 500;
            }

            var keyboardState = inputManager.GetKeyboardState();
            Regex regex = new Regex(@"^[0-9a-zA-Z]{1,1}");
            foreach (var key in keyboardState.GetPressedKeys())
            {
                if (!oldKeyBoardState.GetPressedKeys().Contains(key))
                {
                    if (key == Keys.Back && GameState.PlayerName.Length > 0)
                    {
                        GameState.PlayerName = GameState.PlayerName.Remove(GameState.PlayerName.Length - 1);
                        continue;
                    }
                    if (key == Keys.Enter && GameState.PlayerName.Length > 0)
                    {
                        guiItems.First(gi => gi.GetHeader() == "PLAYER").UpdateText(GameState.PlayerName);
                        GameState.State = State.TransitionNewGameToPosition;
                        return;
                    }
                    if (key.ToString().Length > 1)
                        continue;
                    if (regex.Match(key.ToString()).Success && GameState.PlayerName.Length < 11)
                    {
                        GameState.PlayerName += key.ToString();
                    }
                }
            }

            var mouseState = inputManager.GetMouseState();
            var stringSize = largeFont.MeasureString("OK");
            if (mouseState.Position.X < screenSize.Width * 0.5f + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 80 ||
                mouseState.Position.X > screenSize.Width * 0.5f + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 80 + stringSize.X + 20 ||
                mouseState.Position.Y < screenSize.Height * 0.5f + newGameBackground.Width * 0.5f - 130 ||
                mouseState.Position.Y > screenSize.Height * 0.5f + newGameBackground.Width * 0.5f - 130 + stringSize.Y + 10 ||
                GameState.PlayerName.Length < 1)
            {
                highLightOk = false;
            }
            else
                highLightOk = true;

            if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed && highLightOk)
            {
                guiItems.First(gi => gi.GetHeader() == "PLAYER").UpdateText(GameState.PlayerName);
                GameState.State = State.TransitionNewGameToPosition;
                return;
            }

            oldMouseState = mouseState;
            oldKeyBoardState = keyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                logo,
                new Rectangle(
                    (int)((screenSize.Width - logo.Width) * 0.5f),
                    0,
                    logo.Width,
                    logo.Height),
                Color.White);

            foreach (var guiItem in guiItems)
            {
                guiItem.Draw(spriteBatch);
            }

            var stringSize = largeFont.MeasureString(GameState.HighScore.ToString());
            spriteBatch.DrawString(
                largeFont,
                GameState.HighScore.ToString(),
                new Vector2(screenSize.Width - 256 * 0.5f - stringSize.X * 0.5f, 230),
                Color.White);

            menu.Draw(spriteBatch);

            switch (GameState.State)
            {
                case State.NewGame:
                    DrawNewGameDialog(spriteBatch);
                    break;
                case State.GameOver:
                    DrawGameOverInfo(spriteBatch);
                    break;
            }
        }

        private void SetupGuiItems()
        {
            guiItems.Add(
                new GuiItem(
                    "PLAYER",
                    "",
                    0,
                    138,
                    256,
                    92,
                    grayCorner,
                    darkGrayCorner,
                    grayPixel,
                    darkGrayPixel,
                    largeFont,
                    smallFont,
                    new List<bool>() { false, true, true, false },
                    true)
                );

            guiItems.Add(
                new GuiItem(
                    "TURN",
                    GameState.Turn.ToString(),
                    0,
                    278,
                    256,
                    92,
                    grayCorner,
                    darkGrayCorner,
                    grayPixel,
                    darkGrayPixel,
                    largeFont,
                    smallFont,
                    new List<bool>() { false, true, true, false },
                    true)
                );

            guiItems.Add(
                new GuiItem(
                    "MENU",
                    "",
                    screenSize.Right - 256,
                    screenSize.Bottom - 256,
                    256,
                    256,
                    grayCorner,
                    darkGrayCorner,
                    grayPixel,
                    darkGrayPixel,
                    largeFont,
                    smallFont,
                    new List<bool>() { true, false, false, false },
                    false)
                );

            guiItems.Add(
               new GuiItem(
                   "HIGH SCORE",
                   GameState.HighScorePlayer,
                   screenSize.Width - 256,
                   138,
                   256,
                   130,
                   grayCorner,
                   darkGrayCorner,
                   grayPixel,
                   darkGrayPixel,
                   largeFont,
                   smallFont,
                   new List<bool>() { true, false, false, true },
                   false)
               );
        }

        public void UpdateGuiText(string headerName, string text)
        {
            if (guiItems.Count > 0)
                guiItems.First(gi => gi.GetHeader() == headerName).UpdateText(text);
        }

        private void DrawNewGameDialog(SpriteBatch spriteBatch)
        {
            int screenCenterX = (int)(screenSize.Width * 0.5f);
            int screenCenterY = (int)(screenSize.Height * 0.5f);
            int topBorderNewGameBackground = (int)(screenSize.Height * 0.5f - newGameBackground.Height * 0.5f);
            spriteBatch.Draw(
                newGameBackground,
                new Vector2(
                    screenCenterX - newGameBackground.Width * 0.5f,
                    screenCenterY - newGameBackground.Height * 0.5f),
                Color.White);

            var stringSize = largeFont.MeasureString("NEW GAME");
            spriteBatch.DrawString(
                largeFont,
                "NEW GAME",
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    topBorderNewGameBackground + 70 - stringSize.Y * 0.5f),
                Color.White);

            stringSize = largeFont.MeasureString(GameState.PlayerName);
            spriteBatch.DrawString(
                largeFont,
                GameState.PlayerName,
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    topBorderNewGameBackground + 155),
                Color.White);

            if (drawCursor)
            {
                spriteBatch.Draw(
                    whitePixel,
                    new Rectangle(
                        (int)(screenCenterX + stringSize.X * 0.5f + 5),
                        topBorderNewGameBackground + 150,
                        1,
                        31),
                    new Rectangle(0, 0, 0, 0),
                    Color.White);
            }

            stringSize = largeFont.MeasureString("OK");
            if (highLightOk)
            {
                spriteBatch.Draw(
                        darkGrayPixel,
                        new Rectangle(
                                (int)(screenCenterX + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 70 - 10),
                                (int)(screenCenterY + newGameBackground.Width * 0.5f - 120 - 10),
                                (int)stringSize.X + 20,
                                (int)stringSize.Y + 10),
                        Color.White);
            }

            if (GameState.PlayerName.Length > 0)
            {
                spriteBatch.DrawString(
                    largeFont,
                    "OK",
                    new Vector2(
                        screenCenterX + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 70,
                        screenCenterY + newGameBackground.Width * 0.5f - 120),
                    Color.White);
            }

            var headerColor = new Color(167, 167, 167);
            stringSize = smallFont.MeasureString("ENTER NAME");
            spriteBatch.DrawString(
                smallFont,
                "ENTER NAME",
                new Vector2(
                    screenCenterX + newGameBackground.Width * 0.5f - stringSize.X - 60,
                    374),
                headerColor);
        }

        private void DrawGameOverInfo(SpriteBatch spriteBatch)
        {
            int screenCenterX = (int)(screenSize.Width * 0.5f);
            int screenCenterY = (int)(screenSize.Height * 0.5f);
            int topBorderGameOverScreen = (int)(screenSize.Height * 0.5f - gameOverBackground.Height * 0.5f);
            int distanceBetweenTexts = 100;

            spriteBatch.Draw(
                gameOverBackground,
                new Vector2(
                    screenCenterX - gameOverBackground.Width * 0.5f,
                    screenCenterY - gameOverBackground.Height * 0.5f),
                Color.White);

            var stringSize = largeFont.MeasureString("GAME OVER");
            spriteBatch.DrawString(
                largeFont,
                "GAME OVER",
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    topBorderGameOverScreen + 70 - stringSize.Y * 0.5f),
                Color.White);

            stringSize = largeFont.MeasureString(GameState.Turn.ToString());
            int firstHeaderY = (int)(topBorderGameOverScreen + 165);
            spriteBatch.DrawString(
                largeFont,
                GameState.Turn.ToString(),
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY),
                Color.White);


            stringSize = largeFont.MeasureString(GameState.NrBricksDestroyed.ToString());
            spriteBatch.DrawString(
                largeFont,
                GameState.NrBricksDestroyed.ToString(),
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY + distanceBetweenTexts),
                Color.White);

            var totalMillisecond = GameState.Stopwatch.ElapsedMilliseconds;
            int totalHours = 0;
            int totalMinutes = 0;
            int totalSeconds = 0;
            while (totalMillisecond > 3600000)
            {
                totalHours++;
                totalMillisecond -= 3600000;
            }
            while (totalMillisecond > 60000)
            {
                totalMinutes++;
                totalMillisecond -= 60000;
            }
            while (totalMillisecond > 1000)
            {
                totalSeconds++;
                totalMillisecond -= 1000;
            }

            var totalHoursString = totalHours < 10 ? 0 + totalHours.ToString() : totalHours.ToString();
            var totalMinutesString = totalMinutes < 10 ? 0 + totalMinutes.ToString() : totalMinutes.ToString();
            var totalSecondsString = totalSeconds < 10 ? 0 + totalSeconds.ToString() : totalSeconds.ToString();

            var timeString = $"{totalHoursString}:{totalMinutesString}:{totalSecondsString}";
            stringSize = largeFont.MeasureString(timeString);
            spriteBatch.DrawString(
                largeFont,
                timeString,
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY + 2 * distanceBetweenTexts),
                Color.White);

            stringSize = smallFont.MeasureString("TURN");
            int rightAlign = (int)(screenCenterX + gameOverBackground.Width * 0.5 - 80);
            firstHeaderY = (int)(topBorderGameOverScreen + 140 - stringSize.Y * 0.5f);
            var headerColor = new Color(167, 167, 167);

            spriteBatch.DrawString(
                smallFont,
                "TURN",
                new Vector2(
                    rightAlign - stringSize.X,
                    firstHeaderY),
                headerColor);

            stringSize = smallFont.MeasureString("BRICKS DESTROYED");
            spriteBatch.DrawString(
                smallFont,
                "BRICKS DESTROYED",
                new Vector2(
                    rightAlign - stringSize.X,
                    firstHeaderY + distanceBetweenTexts),
                headerColor);

            stringSize = smallFont.MeasureString("TIME PLAYED");
            spriteBatch.DrawString(
                smallFont,
                "TIME PLAYED",
                new Vector2(
                    rightAlign - stringSize.X,
                    firstHeaderY + 2 * distanceBetweenTexts),
                headerColor);
        }

    }
}
