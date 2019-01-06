using System;
using System.Collections.Generic;
using BallBreaker.Helpers;
using BallBreaker.Managers;
using BallBreaker.Sprites;
using col = Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Collision2D.BasicGeometry;
using Microsoft.Xna.Framework.Input;
using BallBreaker.HelperObjects;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using BallBreaker.GuiObjects;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace BallBreaker.Screens
{
    public enum Walls { Left, Right, Top };

    public class GameScreen : IScreen
    {
        private enum State { Turn, TurnTransition, Positioning, Aiming, EndOfTurn, GameOver, NewGame };
        private State gameState;

        private delegate void MenuItemHandler();

        private List<Ball> balls;
        private Dictionary<Walls, ScreenBorder> borders;
        private ContentManager content;
        private InputManager inputManager;
        private bool debug = false;
        private List<List<Brick>> gameMatrix;
        private int turn = 1;

        private Texture2D whitePixel;
        private Texture2D grayPixel;
        private Texture2D darkGrayPixel;
        private Texture2D brickImage;
        private Texture2D ballImage;
        private Texture2D playAreaBackground;
        private Texture2D background;
        private Texture2D logo;
        private Texture2D grayCorner;
        private Texture2D darkGrayCorner;
        private Texture2D aimingBall;
        private Texture2D gameOverBackground;
        private Texture2D newGameBackground;

        private SpriteFont font;
        private SpriteFont lcdFont;
        private SpriteFont fontSmall;

        private SoundEffect bounce;
        private SoundEffect slide;

        private col.Point brickSize = new col.Point(64, 64);
        private Vector aimDirection;
        private Rectangle screenSize;
        private col.Point playArea;
        private BallFactory ballFactory;
        private SoundManager soundManager;

        private bool playCollissionSound = false;
        private bool validAim = false;
        private bool drawCursor = false;
        private bool highLightOk = false;
        private double cursorTimer = 500;
        private bool isGameOver = false;
        private int nrBricksDestroyed = 0;
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
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BounceMadness");
            Directory.CreateDirectory(folderPath);
            SetupHighScore();
            playArea = new col.Point(playAreaWidth, playAreaHeight);
            guiItems = new List<GuiItem>();
            stopWatch = new Stopwatch();
            ballFactory = new BallFactory
            {
                Position = new col.Point(600, 750),
                BallRadius = 10
            };
            oldKeyBoardState = inputManager.GetKeyboardState();
            oldMouseState = inputManager.GetMouseState();
            SetupBorders();
            StartNewGame();
            stopWatch.Start();
        }

        private void SetupHighScore()
        {
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

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(0, 0, screenSize.Width, screenSize.Height), Color.White);
            spriteBatch.Draw(
                playAreaBackground,
                new Rectangle(
                    (int)borders[Walls.Left].BoundingBox.Right,
                    (int)borders[Walls.Top].BoundingBox.Bottom,
                    playAreaBackground.Width,
                    playAreaBackground.Height),
                Color.White);

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
                case State.Aiming:
                    DrawGameMatrix(spriteBatch);
                    ballFactory.Draw(spriteBatch);
                    if (validAim)
                    {
                        DrawAimingBalls(spriteBatch, aimDirection);
                        validAim = false;
                    }
                    break;
                case State.TurnTransition:
                    DrawGameMatrix(spriteBatch);
                    break;
                case State.Positioning:
                    DrawGameMatrix(spriteBatch);
                    ballFactory.Draw(spriteBatch);
                    break;
                case State.Turn:
                    DrawGameMatrix(spriteBatch);
                    foreach (var ball in balls)
                        ball.Draw(spriteBatch);
                    if (playCollissionSound)
                        soundManager.PlayBounceSound();
                    break;
                case State.GameOver:
                    DrawGameMatrix(spriteBatch);
                    DrawGameOverInfo(spriteBatch);
                    break;
            }

            playCollissionSound = false;
        }

        private void DrawGameMatrix(SpriteBatch spriteBatch)
        {
            foreach (var row in gameMatrix)
            {
                if (row == null)
                    continue;
                foreach (var brick in row)
                {
                    if (brick != null)
                    {
                        brick.Image = brickImage;
                        brick.Font = fontSmall;
                        brick.Draw(spriteBatch);
                    }
                }
            }
        }
        
        public void LoadContent()
        {
            balls = new List<Ball>();
            ballImage = content.Load<Texture2D>("Images/Ball");
            ballFactory.Image = ballImage;
            
            // Brick
            brickImage = content.Load<Texture2D>("Images/BrickRed");

            // Font
            font = content.Load<SpriteFont>("GameFont");
            fontSmall = content.Load<SpriteFont>("Fonts/GameFontSmall");
            lcdFont = content.Load<SpriteFont>("Fonts/LCDFont");

            // Aiming
            aimingBall = content.Load<Texture2D>("Images/AimBall");

            // Backgrounds
            playAreaBackground = content.Load<Texture2D>("Images/PlayAreaBackground");
            background = content.Load<Texture2D>("Images/Background");
            gameOverBackground = content.Load<Texture2D>("Images/GameOverBackground");
            newGameBackground = content.Load<Texture2D>("Images/NewGameBackground");

            // Soundeffects
            bounce = content.Load<SoundEffect>("Sound/Bounce");
            slide = content.Load<SoundEffect>("Sound/Slide");
            soundManager = new SoundManager(bounce);

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
                    turn.ToString(),
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

        public void UnloadContent()
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            switch (gameState)
            {
                case State.NewGame:
                    NewGameUpdate(gameTime);
                    break;
                case State.Turn:
                    DuringTurnUpdate(gameTime);
                    break;
                case State.Positioning:
                    PositioningUpdate();
                    break;
                case State.Aiming:
                    AimingUpdate();
                    break;
                case State.TurnTransition:
                    TurnTransitionUpdate(gameTime);
                    break;
            }

            menu.Update(gameTime);
        }

        private void SetupBorders()
        {
            borders = new Dictionary<Walls, ScreenBorder>();

            var sideBorderWidth = (screenSize.Width - playArea.X) / 2;
            var leftBorder = new ScreenBorder();
            leftBorder.BoundingBox = new Aabb(0, 0, sideBorderWidth, screenSize.Height);
            borders.Add(Walls.Left, leftBorder);

            var rightBorder = new ScreenBorder();
            rightBorder.BoundingBox = new Aabb(screenSize.Width - sideBorderWidth, 0, sideBorderWidth, screenSize.Height);
            borders.Add(Walls.Right, rightBorder);

            var topBorder = new ScreenBorder();
            topBorder.BoundingBox = new Aabb(new col.Point(600, 68), 1200, 136);
            borders.Add(Walls.Top, topBorder);
        }

        private void StartNewGame()
        {
            gameState = State.NewGame;

            ballFactory.BallsInFactory = 1;
            ballFactory.IsFirstBall = true;

            nrBricksDestroyed = 0;
            turn = 1;

            if (guiItems.Count > 1)
            {
                guiItems.First(gi => gi.GetHeader() == "TURN").UpdateText("1");
                guiItems.First(gi => gi.GetHeader() == "HIGH SCORE").UpdateText(highScorePlayer);
            }
        }

        private void TransitionNewGameToPosition()
        {
            gameMatrix = new List<List<Brick>>();
            var totalNrRows = 10;
            for (int i = 0; i < totalNrRows; i++)
            {
                gameMatrix.Add(null);
            }

            gameMatrix[0] = SetupTopRow();
            gameState = State.Positioning;
        }

        private void EndTurnClicked()
        {
            balls.Clear();
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

            stringSize = font.MeasureString(turn.ToString());
            int firstHeaderY = (int)(topBorderGameOverScreen + 165);
            spriteBatch.DrawString(
                font,
                turn.ToString(),
                new Vector2(
                    screenCenterX - stringSize.X * 0.5f,
                    firstHeaderY),
                Color.White);


            stringSize = font.MeasureString(nrBricksDestroyed.ToString());
            spriteBatch.DrawString(
                font,
                nrBricksDestroyed.ToString(),
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

        private void DrawAimingBalls(SpriteBatch spriteBatch, Vector aim)
        {
            int nrBalls = 10;
            float xStep = aim.X / nrBalls;
            float yStep = aim.Y / nrBalls;
            for(int i = nrBalls; i > 0; i--)
            {
                spriteBatch.Draw(
                    aimingBall,
                    new Rectangle(
                        (int)(ballFactory.Position.X + i * xStep - aimingBall.Width * 0.5f),
                        (int)(ballFactory.Position.Y + i * yStep - aimingBall.Height * 0.5f),
                        aimingBall.Width,
                        aimingBall.Height),
                    Color.White);
            }
        }

        private List<Brick> SetupTopRow()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            int nrColumns = (int)(playArea.X / brickSize.X);
            var maxValue = turn * (nrColumns - 1);
            var filledCells = rnd.Next(1, nrColumns);
            
            List<int> cellIndexes = new List<int>();
            for (int i = 0; i < nrColumns; i++)
                cellIndexes.Add(i);

            var cellsToFill = new List<Cell>();
            for (int i = 0; i < filledCells; i++)
            {
                var index = rnd.Next(0, cellIndexes.Count);
                var cell = new Cell();
                cell.Index = cellIndexes[index];
                cellIndexes.RemoveAt(index);

                int potentialMultiplier;
                if (filledCells - cellsToFill.Count - 1 > 2)
                    potentialMultiplier = 3;
                else if (filledCells - cellsToFill.Count - 1 > 1)
                    potentialMultiplier = 2;
                else
                    potentialMultiplier = 1;

                cell.Value = rnd.Next(1, potentialMultiplier + 1) * turn;

                cellsToFill.Add(cell);
            }

            return AddBrickRow(cellsToFill, nrColumns);
        }

        private List<Brick> AddBrickRow(List<Cell> cellInfo, int nrColumns)
        {
            var brickRow = new List<Brick>();
            for (int i = 0; i < nrColumns; i++)
            {
                if (cellInfo.Any(c => c.Index == i)) {
                    var brick = new Brick();
                    brick.BoundingBox = new Aabb(
                    new col.Point(
                        i * brickSize.X + brickSize.X * 0.5f + borders[Walls.Left].BoundingBox.Width,
                        brickSize.Y + brickSize.Y * 0.5f + borders[Walls.Top].BoundingBox.Height),
                    brickSize.X, brickSize.Y);
                    brick.Counter = cellInfo.First(c => c.Index == i).Value;
                    brickRow.Add(brick);
                }
                else
                {
                    brickRow.Add(null);
                }
            }
            return brickRow;
        }

        private void UpdateAimDirection(MouseState mouseState)
        {
            aimDirection = new Vector(
                    mouseState.Position.X - ballFactory.Position.X,
                    mouseState.Position.Y - ballFactory.Position.Y);

            if (aimDirection.Angle > -Math.PI * 0.95
                && aimDirection.Angle < -Math.PI * 0.05
                && mouseState.Position.X > borders[Walls.Left].BoundingBox.Right
                && mouseState.Position.X < borders[Walls.Right].BoundingBox.Left
                && mouseState.Position.Y > borders[Walls.Top].BoundingBox.Bottom)
                validAim = true;
            else
                validAim = false;
        }

        private void AimingUpdate()
        {
            var mouseState = inputManager.GetMouseState();
            UpdateAimDirection(mouseState);
            if (mouseState.LeftButton == ButtonState.Released)
            {
                if (aimDirection != null && validAim && aimDirection.Length > 100)
                {
                    ballFactory.InitialVelocity = aimDirection;
                    ballFactory.BallsInFactory = turn;
                    ballFactory.IsFirstBall = true;
                    gameState = State.Turn;
                }
                else
                    gameState = State.Positioning;

                validAim = false;
            }   
        }

        private void TurnTransitionUpdate(GameTime gameTime)
        {
            float step = (float)gameTime.ElapsedGameTime.Milliseconds * 0.2f;
            bool transitionCompleted = false;

            int totalBricks = 0;
            for (int i = gameMatrix.Count - 1; i > -1; i--)
            {
                if (gameMatrix[i] == null)
                    continue;
                
                foreach (var brick in gameMatrix[i])
                {
                    if (brick == null)
                        continue;

                    totalBricks++;
                    var moveStep = step;

                    if (brick.BoundingBox.Center.Y + step < brick.NewYPos)
                        brick.BoundingBox.Center.Y += moveStep;
                    else
                    {
                        brick.BoundingBox.Center.Y = brick.NewYPos;
                        brick.OldYPos = brick.NewYPos;
                        transitionCompleted = true;
                        continue;
                    }
                }
            }

            if (!transitionCompleted && totalBricks > 0)
                return;

            if (isGameOver)
            {
                gameState = State.GameOver;
                stopWatch.Stop();
                return;
            }

            for (int i = gameMatrix.Count - 1; i > 0; i--)
                gameMatrix[i] = gameMatrix[i - 1];

            turn++;
            guiItems.First(g => g.GetHeader() == "TURN").UpdateText(turn.ToString());
            if (turn > highScore)
            {
                highScore = turn;
                highScorePlayer = playerName;
                guiItems.First(g => g.GetHeader() == "HIGH SCORE").UpdateText(highScorePlayer);
                SaveHighScore();
            }

            gameMatrix[0] = SetupTopRow();
            if (gameMatrix.Count > 8)
                gameMatrix.RemoveAt(gameMatrix.Count - 1);
            gameState = State.Positioning;
            ballFactory.IsFirstBall = true;
        }

        private void PositioningUpdate()
        {
            var mouseState = inputManager.GetMouseState();
            var mousePosition = mouseState.Position;
            var mouseX = 0f;
            if (mousePosition.X > 1200 - borders[Walls.Right].BoundingBox.Width - ballFactory.BallRadius)
                mouseX = 1200 - borders[Walls.Right].BoundingBox.Width - ballFactory.BallRadius - 1;
            else if (mousePosition.X < borders[Walls.Left].BoundingBox.Width + ballFactory.BallRadius)
                mouseX = borders[Walls.Left].BoundingBox.Width + ballFactory.BallRadius + 1;
            else
                mouseX = mousePosition.X;

            ballFactory.Position = new col.Point(
                mouseX,
                screenSize.Height - ballFactory.BallRadius * 2);

            if (mouseState.LeftButton == ButtonState.Pressed)
                gameState = State.Aiming;
        }

        private void DuringTurnUpdate(GameTime gameTime)
        {
            if (ballFactory.IsFirstBall)
            {
                balls.Add(new Ball()
                {
                    BoundingCircle = new Circle(ballFactory.Position, 10),
                    Velocity = ballFactory.InitialVelocity,
                    Image = ballImage
                });

                ballFactory.TimeSinceLastBall = 0;
                ballFactory.IsFirstBall = false;
                ballFactory.BallsInFactory--;
            }

            if (ballFactory.BallsInFactory > 0)
            {
                ballFactory.TimeSinceLastBall += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (ballFactory.TimeSinceLastBall > 50)
                {
                    balls.Add(new Ball()
                    {
                        BoundingCircle = new Circle(ballFactory.Position, 10),
                        Velocity = ballFactory.InitialVelocity,
                        Image = ballImage
                    });

                    ballFactory.TimeSinceLastBall = 0;
                    ballFactory.BallsInFactory--;
                }
            }

            for (int i = 0; i < balls.Count; i++)
            {
                if (Math.Abs(balls[i].Velocity.Angle) < 0.1)
                    balls[i].Velocity.Angle = (float)Math.PI * 0.01f;
                if (Math.Abs(balls[i].Velocity.Angle - Math.PI) < 0.1)
                    balls[i].Velocity.Angle = (float)Math.PI * 0.99f;
                balls[i].Velocity = balls[i].Velocity.Normalize() * balls[i].Speed;
                balls[i].MovementStep = balls[i].Speed / 1000 * gameTime.ElapsedGameTime.Milliseconds;

                if (balls[i].BoundingCircle.Center.Y > 800 - 20)
                {
                    balls.Remove(balls[i]);
                    continue;
                }

                if (balls[i].BoundingCircle.Center.X > borders[Walls.Right].BoundingBox.Left + 20
                    || balls[i].BoundingCircle.Center.X < borders[Walls.Left].BoundingBox.Right - 20
                    || balls[i].BoundingCircle.Center.Y < borders[Walls.Top].BoundingBox.Bottom - 20)
                {
                    balls.Remove(balls[i]);
                    continue;
                }

                CollisionChecks.GenerateWallSegments(ballFactory.BallRadius, gameMatrix, borders);
                ResetBricksCollissionStatus();
                HandleCollisions(balls[i], gameTime);
            }

            if (balls.Count == 0)
                EndOfTurn();

            UpdateRows();

            foreach (var ball in balls)
                ball.Update(gameTime);
        }

        private void NewGameUpdate(GameTime gameTime)
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
                        continue;
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
            }

            oldMouseState = mouseState;
            oldKeyBoardState = keyboardState;
        }

        private void ResetBricksCollissionStatus()
        {
            foreach (var list in gameMatrix)
            {
                if (list == null)
                    continue;

                foreach (var brick in list)
                {
                    if (brick == null)
                        continue;

                    brick.HaveCollidedThisTurn = false;
                }
            }
        }

        private void HandleCollisions(Ball ball, GameTime gameTime)
        {
            bool allResolved = false;

            CollisionObject lastResolved = null;
            while (!allResolved)
            {
                List<CollisionObject> intersections = new List<CollisionObject>();
                allResolved = true;

                var wallIntObject = CollisionChecks.WallCollision(
                    ball,
                    gameMatrix,
                    borders);

                if (wallIntObject.IntersectionObject.Intersects)
                    intersections.Add(wallIntObject);

                intersections.AddRange(GetIntersections(ball));

                if (intersections.Count > 0)
                {
                    if (intersections.Count > 1)
                    {
                        intersections.Sort();
                        if (intersections[0].IntersectionObject.IntersectionPoint.X == intersections[1].IntersectionObject.IntersectionPoint.X &&
                            intersections[0].IntersectionObject.IntersectionPoint.Y == intersections[1].IntersectionObject.IntersectionPoint.Y)
                        {
                            if (intersections[1].IntersectionObject.IntersectionPoint.Distance(ball.BoundingCircle.Center) <
                                intersections[0].IntersectionObject.IntersectionPoint.Distance(ball.BoundingCircle.Center))
                            {
                                intersections[0] = intersections[1];
                            }
                        }
                    }
                    if (intersections[0].IntersectionObject.IntersectionType == Collision2D.HelperObjects.IntersectionType.Wall)
                        CollisionChecks.ResolveWallCollission(ball, intersections[0]);
                    else
                    {
                        CollisionChecks.ResolveBrickCollission(ball, intersections[0]);
                        intersections[0].Brick.Counter--;
                    }

                    lastResolved = intersections[0];
                    allResolved = false;
                    playCollissionSound = true;
                }
                else
                    allResolved = true;
            }
        }

        private void UpdateRows()
        {
            foreach (var row in gameMatrix)
            {
                if (row == null)
                    continue;
                for (int i = 0; i < row.Count; i++)
                {
                    if (row[i] != null && row[i].Counter < 1)
                    {
                        row[i] = null;
                        nrBricksDestroyed++;
                    }
                }
            }
        }

        private List<CollisionObject> GetIntersections(Ball ball)
        {
            var intersections = new List<CollisionObject>();
            for (int i = 0; i < gameMatrix.Count; i++)
            {
                if (gameMatrix[i] == null)
                    continue;
                for (int j = 0; j < gameMatrix[i].Count; j++)
                {
                    if (gameMatrix[i][j] == null || gameMatrix[i][j].HaveCollidedThisTurn)
                        continue;
                    var movementStepVector = new Vector(ball.Velocity * ball.MovementStep);
                    if (Collision2D.HelperObjects.DistanceHelper.WithinDistance(
                        ball.BoundingCircle.Center,
                        gameMatrix[i][j].BoundingBox.Center,
                        ball.BoundingCircle.Radius + gameMatrix[i][j].BoundingBox.DistanceCenterToCorner + movementStepVector.Length))
                    {
                        bool[,] sourounding = new bool[3, 3];
                        sourounding[0, 0] = InBoundsAndNotNull(i - 1, j - 1);
                        sourounding[0, 1] = InBoundsAndNotNull(i, j - 1);
                        sourounding[0, 2] = InBoundsAndNotNull(i + 1, j - 1);
                        sourounding[1, 0] = InBoundsAndNotNull(i - 1, j);
                        sourounding[1, 1] = false;
                        sourounding[1, 2] = InBoundsAndNotNull(i + 1, j);
                        sourounding[2, 0] = InBoundsAndNotNull(i - 1, j + 1);
                        sourounding[2, 1] = InBoundsAndNotNull(i, j + 1);
                        sourounding[2, 2] = InBoundsAndNotNull(i + 1, j + 1);

                        var intersectionObject = CollisionChecks.BrickCollision(ball, gameMatrix[i][j], sourounding, debug);
                        if (intersectionObject.IntersectionObject.Intersects)
                        {
                            intersections.Add(intersectionObject);
                            gameMatrix[i][j].HaveCollidedThisTurn = true;
                        }
                    }
                }
            }
            return intersections;
        }

        private bool InBoundsAndNotNull(int idOuter, int idInner)
        {
            int nrColumns = (int)(playArea.X / brickSize.X);

            if (idInner < 0 || idInner > nrColumns - 1)
                return true;
            if (idOuter < 0 || 
                idOuter > gameMatrix.Count - 1 || 
                gameMatrix[idOuter] == null ||
                gameMatrix[idOuter][idInner] == null)
                return false;

            return true;
        }

        private void SetupTransition()
        {
            for (int i = gameMatrix.Count - 1; i > -1; i--)
            {
                if (gameMatrix[i] == null)
                    continue;

                foreach (var brick in gameMatrix[i])
                {
                    if (brick != null)
                        brick.OldYPos = brick.BoundingBox.Center.Y;
                }
            }

            slide.Play();
        }

        private void EndOfTurn()
        {
            isGameOver = false;
            if (gameMatrix[gameMatrix.Count - 1] != null)
            {
                foreach (var brick in gameMatrix[gameMatrix.Count - 1])
                {
                    if (brick != null)
                    {
                        isGameOver = true;
                    }
                }
            }

            gameState = State.TurnTransition;
            SetupTransition();
        }

        private List<Brick> EmptyRow()
        {
            var row = new List<Brick>();
            for (int i = 0; i < (int)(playArea.X / brickSize.X); i++)
                row.Add(null);
            return row;
        }

        private void ExitClicked()
        {
            Exit.Invoke(null, null);
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
        
        private class Cell
        {
            public int Value { get; set; } = 0;
            public int Index { get; set; }
        }
    }
}
