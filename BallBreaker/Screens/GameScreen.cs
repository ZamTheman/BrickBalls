using System;
using System.Collections.Generic;
using BallBreaker.Managers;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BallBreaker.GuiObjects;
using static BallBreaker.HelperObjects.Enums;

namespace BallBreaker.Screens
{
    public class GameScreen : IScreen
    {
        private State gameState;

        private delegate void MenuItemHandler();

        private ContentManager content;
        private InputManager inputManager;
        private SoundManager soundManager;
        private GameBoard gameBoard;

        private Rectangle screenSize;
        private Texture2D whitePixel;
        private Texture2D grayPixel;
        private Texture2D darkGrayPixel;
        private Texture2D background;
        private Texture2D logo;
        private Texture2D grayCorner;
        private Texture2D darkGrayCorner;
        private Texture2D gameOverBackground;
        private Texture2D newGameBackground;

        private SpriteFont font;
        private SpriteFont fontSmall;

        private bool drawCursor = false;
        private bool highLightOk = false;
        private double cursorTimer = 500;
        
        private string playerName = string.Empty;
        private string highScorePlayer = string.Empty;
        private int highScore = 1;
        private KeyboardState oldKeyBoardState;
        private MouseState oldMouseState;

        private Menu menu;
        private List<GuiItem> guiItems;

        private Stopwatch stopWatch;
        private string folderPath;

        public event EventHandler Exit;

        public GameScreen(ContentManager content,
            InputManager inputManager,
            Rectangle screenSize,
            int playAreaWidth,
            int playAreaHeight)
        {
            this.content = content;
            this.inputManager = inputManager;
            this.screenSize = screenSize;
            soundManager = new SoundManager();
            gameBoard = new GameBoard(
                playAreaWidth,
                playAreaHeight,
                screenSize,
                soundManager,
                inputManager);
            gameBoard.Turn = 1;
            SetupHighScore();
            guiItems = new List<GuiItem>();
            stopWatch = new Stopwatch();
            oldKeyBoardState = inputManager.GetKeyboardState();
            oldMouseState = inputManager.GetMouseState();
            gameBoard.StateChanged += StateChangedHandler;
            StartNewGame();
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(0, 0, screenSize.Width, screenSize.Height), Color.White);
            gameBoard.Draw(spriteBatch, gameState);
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

            var stringSize = font.MeasureString(highScore.ToString());
            spriteBatch.DrawString(
                font,
                highScore.ToString(),
                new Vector2(screenSize.Width - 256 * 0.5f - stringSize.X * 0.5f, 230),
                Color.White);

            menu.Draw(spriteBatch);
            
            switch (gameState)
            {
                case State.NewGame:
                    DrawNewGameDialog(spriteBatch);
                    break;
                case State.GameOver:
                    DrawGameOverInfo(spriteBatch);
                    break;
            }
        }
        
        public void LoadContent()
        {
            soundManager.LoadContent(content);
            gameBoard.LoadContent(content);
            
            // Font
            font = content.Load<SpriteFont>("GameFont");
            fontSmall = content.Load<SpriteFont>("Fonts/GameFontSmall");
            
            // Backgrounds
            background = content.Load<Texture2D>("Images/Background");
            gameOverBackground = content.Load<Texture2D>("Images/GameOverBackground");
            newGameBackground = content.Load<Texture2D>("Images/NewGameBackground");

            // GUI Elements
            logo = content.Load<Texture2D>("Images/Logo");
            whitePixel = content.Load<Texture2D>("Images/WhitePixel");
            grayPixel = content.Load<Texture2D>("Images/GrayPixel");
            darkGrayPixel = content.Load<Texture2D>("Images/DarkGrayPixel");
            grayCorner = content.Load<Texture2D>("Images/GrayCorner");
            darkGrayCorner = content.Load<Texture2D>("Images/DarkGrayCorner");
            menu = new Menu(
                new Dictionary<string, Delegate> { { "NEW GAME", new MenuItemHandler(StartNewGame) }, { "END TURN", new MenuItemHandler(EndTurnClicked) }, { "EXIT", new MenuItemHandler(ExitClicked) } },
                new Vector2(screenSize.Right - 128, screenSize.Bottom - 170),
                font,
                darkGrayPixel);

            SetupGuiItems();
        }

        public void UnloadContent()
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            var initialGameState = gameState;
            gameBoard.Update(gameTime, gameState);
            menu.Update(gameTime);
            if (gameState == State.NewGame)
            {
                UpdateNewGame(gameTime);
            }

            if (gameState == State.Positioning && initialGameState == State.TurnTransition)
            {
                guiItems.First(g => g.GetHeader() == "TURN").UpdateText(gameBoard.Turn.ToString());
                if (gameBoard.Turn > highScore)
                {
                    highScore = gameBoard.Turn;
                    highScorePlayer = playerName;
                    guiItems.First(g => g.GetHeader() == "HIGH SCORE").UpdateText(highScorePlayer);
                    SaveHighScore();
                }
            }

            if (gameState == State.GameOver)
                stopWatch.Stop();
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
                    if (key == Keys.Back)
                    {
                        playerName = playerName.Remove(playerName.Length - 1);
                        continue;
                    }
                    if (key == Keys.Enter && playerName.Length > 0)
                    {
                        guiItems.First(gi => gi.GetHeader() == "PLAYER").UpdateText(playerName);
                        TransitionNewGameToPosition();
                        return;
                    }
                    if (key.ToString().Length > 1)
                        continue;
                    if (regex.Match(key.ToString()).Success && playerName.Length < 11)
                    {
                        playerName += key.ToString();
                    }
                }
            }

            var mouseState = inputManager.GetMouseState();
            var stringSize = font.MeasureString("OK");
            if (mouseState.Position.X < screenSize.Width * 0.5f + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 80 ||
                mouseState.Position.X > screenSize.Width * 0.5f + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 80 + stringSize.X + 20 ||
                mouseState.Position.Y < screenSize.Height * 0.5f + newGameBackground.Width * 0.5f - 130 ||
                mouseState.Position.Y > screenSize.Height * 0.5f + newGameBackground.Width * 0.5f - 130 + stringSize.Y + 10 ||
                playerName.Length < 1)
            {
                highLightOk = false;
            }
            else
                highLightOk = true;

            if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed && highLightOk)
            {
                guiItems.First(gi => gi.GetHeader() == "PLAYER").UpdateText(playerName);
                TransitionNewGameToPosition();
                return;
            }

            oldMouseState = mouseState;
            oldKeyBoardState = keyboardState;
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

            var stringSize = font.MeasureString("NEW GAME");
            spriteBatch.DrawString(
                font,
                "NEW GAME",
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    topBorderNewGameBackground + 70 - stringSize.Y * 0.5f),
                Color.White);

            stringSize = font.MeasureString(playerName);
            spriteBatch.DrawString(
                font,
                playerName,
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

            stringSize = font.MeasureString("OK");
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

            if (playerName.Length > 0)
            {
                spriteBatch.DrawString(
                    font,
                    "OK",
                    new Vector2(
                        screenCenterX + newGameBackground.Width * 0.5f - stringSize.X * 0.5f - 70,
                        screenCenterY + newGameBackground.Width * 0.5f - 120),
                    Color.White);
            }
            
            var headerColor = new Color(167, 167, 167);
            stringSize = fontSmall.MeasureString("ENTER NAME");
            spriteBatch.DrawString(
                fontSmall,
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

            var stringSize = font.MeasureString("GAME OVER");
            spriteBatch.DrawString(
                font,
                "GAME OVER",
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    topBorderGameOverScreen + 70 - stringSize.Y * 0.5f),
                Color.White);

            stringSize = font.MeasureString(gameBoard.Turn.ToString());
            int firstHeaderY = (int)(topBorderGameOverScreen + 165);
            spriteBatch.DrawString(
                font,
                gameBoard.Turn.ToString(),
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY),
                Color.White);


            stringSize = font.MeasureString(gameBoard.NrBricksDestroyed.ToString());
            spriteBatch.DrawString(
                font,
                gameBoard.NrBricksDestroyed.ToString(),
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY + distanceBetweenTexts),
                Color.White);

            var totalMillisecond = stopWatch.ElapsedMilliseconds;
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
            stringSize = font.MeasureString(timeString);
            spriteBatch.DrawString(
                font,
                timeString,
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY + 2 * distanceBetweenTexts),
                Color.White);

            stringSize = fontSmall.MeasureString("TURN");
            int rightAlign = (int)(screenCenterX + gameOverBackground.Width * 0.5 - 80);
            firstHeaderY = (int)(topBorderGameOverScreen + 140 - stringSize.Y * 0.5f);
            var headerColor = new Color(167, 167, 167);

            spriteBatch.DrawString(
                fontSmall,
                "TURN",
                new Vector2(
                    rightAlign - stringSize.X,
                    firstHeaderY),
                headerColor);

            stringSize = fontSmall.MeasureString("BRICKS DESTROYED");
            spriteBatch.DrawString(
                fontSmall,
                "BRICKS DESTROYED",
                new Vector2(
                    rightAlign - stringSize.X,
                    firstHeaderY + distanceBetweenTexts),
                headerColor);

            stringSize = fontSmall.MeasureString("TIME PLAYED");
            spriteBatch.DrawString(
                fontSmall,
                "TIME PLAYED",
                new Vector2(
                    rightAlign - stringSize.X,
                    firstHeaderY + 2 * distanceBetweenTexts),
                headerColor);
        }

        private void TransitionNewGameToPosition()
        {
            gameBoard.SetupNewGame();
            gameState = State.Positioning;
            stopWatch.Reset();
            stopWatch.Start();
        }

        private void SetupHighScore()
        {
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BounceMadness");
            Directory.CreateDirectory(folderPath);
            var hs = GetHighScore();

            if (hs == string.Empty)
            {
                highScorePlayer = playerName;
                highScore = 1;
                return;
            }

            highScorePlayer = hs.Split(',')[0];
            highScore = int.Parse(hs.Split(',')[1]);
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
                    font,
                    fontSmall,
                    new List<bool>() { false, true, true, false },
                    true)
                );

            guiItems.Add(
                new GuiItem(
                    "TURN",
                    gameBoard.Turn.ToString(),
                    0,
                    278,
                    256,
                    92,
                    grayCorner,
                    darkGrayCorner,
                    grayPixel,
                    darkGrayPixel,
                    font,
                    fontSmall,
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
                    font,
                    fontSmall,
                    new List<bool>() { true, false, false, false },
                    false)
                );

            guiItems.Add(
               new GuiItem(
                   "HIGH SCORE",
                   highScorePlayer,
                   screenSize.Width - 256,
                   138,
                   256,
                   130,
                   grayCorner,
                   darkGrayCorner,
                   grayPixel,
                   darkGrayPixel,
                   font,
                   fontSmall,
                   new List<bool>() { true, false, false, true},
                   false)
               );
        }

        private void StartNewGame()
        {
            gameState = State.NewGame;

            if (guiItems.Count > 1)
            {
                guiItems.First(gi => gi.GetHeader() == "TURN").UpdateText("1");
                guiItems.First(gi => gi.GetHeader() == "HIGH SCORE").UpdateText(highScorePlayer);
            }
        }

        private void SaveHighScore()
        {
            string filePath = Path.Combine(folderPath, "hs.txt");
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(playerName + "," + highScore);
            }
        }

        private string GetHighScore()
        {
            string filePath = Path.Combine(folderPath, "hs.txt");
            string hs = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    hs = sr.ReadLine();
                }
            }
            catch
            {
                return hs = string.Empty;
            }

            return hs;
        }

        private void EndTurnClicked()
        {
            gameBoard.ClearBalls();
        }

        private void ExitClicked() => Exit.Invoke(null, null);

        private void StateChangedHandler(object sender, StateChangedEventArgs state) => gameState = state.state;
    }
}
