using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using BallBreaker.Sprites;
using col = Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using BallBreaker.Helpers;
using BallBreaker.HelperObjects;
using static BallBreaker.HelperObjects.Enums;

namespace BallBreaker.Managers
{
    public class GameManager
    {
        private SoundManager soundManager;
        private InputManager inputManager;

        private Rectangle screenSize;
        private Rectangle playArea;
        private Dictionary<Walls, ScreenBorder> borders;
        private List<Ball> balls;
        private List<List<Brick>> gameMatrix;
        private List<ParticleFactory> particleFactories;
        private BallFactory ballFactory;
        private col.Point brickSize = new col.Point(64, 64);

        private col.Vector aimDirection;

        private Texture2D brickImage;
        private Texture2D brickShading;
        private Texture2D ballImage;
        private Texture2D playAreaBackground;
        private Texture2D aimingBall;
        private Texture2D whitePixel;

        private SpriteFont largeFont;
        private SpriteFont smallFont;
        
        private bool isGameOver = false;
        private bool playCollissionSound = false;
        private bool successPlayedThisTurn = false;
        private bool validAim = false;

        public GameManager(
            int playAreaWidth,
            int playAreaHeight,
            Rectangle screenSize,
            SoundManager soundManager,
            InputManager inputManager)
        {
            this.soundManager = soundManager;
            this.inputManager = inputManager;
            this.screenSize = screenSize;
            playArea = new Rectangle(
                (screenSize.Width - playAreaWidth) / 2,
                136,
                playAreaWidth,
                playAreaHeight);
            SetupBorders();
            balls = new List<Ball>();
            ballFactory = new BallFactory
            {
                Position = new col.Point(600, 750),
                BallRadius = 10
            };
            particleFactories = new List<ParticleFactory>();
            SetupNewGame();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                playAreaBackground,
                new Vector2(
                    playArea.Left,
                    playArea.Top),
                Color.White * 0.95f);
            
            switch (GameState.State)
            {
                case State.Turn:
                    DrawGameMatrix(spriteBatch);
                    foreach (var ball in balls)
                        ball.Draw(spriteBatch);
                    break;
                case State.TurnTransition:
                    DrawGameMatrix(spriteBatch);
                    ballFactory.Draw(spriteBatch);
                    break;
                case State.Positioning:
                    DrawGameMatrix(spriteBatch);
                    ballFactory.Draw(spriteBatch);
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
                case State.EndOfTurn:
                    DrawGameMatrix(spriteBatch);
                    ballFactory.Draw(spriteBatch);
                    break;
                case State.GameOver:
                    break;
                case State.NewGame:
                    break;
                default:
                    break;
            }

            foreach (var particleFactory in particleFactories)
                particleFactory.Draw(spriteBatch, whitePixel);
            
            if (playCollissionSound)
                soundManager.PlaySound(Sounds.Bounce);

            playCollissionSound = false;
        }

        public void LoadContent(ContentManager content)
        {
            ballImage = content.Load<Texture2D>("Images/Ball");
            ballFactory.Image = ballImage;

            // Fonts
            largeFont = content.Load<SpriteFont>("GameFont");
            smallFont = content.Load<SpriteFont>("Fonts/GameFontSmall");

            // Brick
            brickImage = content.Load<Texture2D>("Images/BrickRed");
            brickShading = content.Load<Texture2D>("Images/BrickShading");

            // Aiming
            aimingBall = content.Load<Texture2D>("Images/AimBall");

            // Pixels
            whitePixel = content.Load<Texture2D>("Images/WhitePixel");

            playAreaBackground = content.Load<Texture2D>("Images/PlayAreaBackground");
        }

        public void Update(GameTime gameTime)
        {
            switch (GameState.State)
            {
                case State.NewGame:
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

            for (int i = particleFactories.Count; i > 0; i--)
            {
                if (particleFactories[i - 1].ParticleCount == 0)
                    particleFactories.Remove(particleFactories[i - 1]);

                else
                    particleFactories[i - 1].Update(gameTime);
            }   
        }

        public void SetupNewGame()
        {
            ballFactory.BallsInFactory = 1;
            ballFactory.IsFirstBall = true;
            GameState.NrBricksDestroyed = 0;
            GameState.Turn = 1;
            gameMatrix = new List<List<Brick>>();
            var totalNrRows = 10;
            for (int i = 0; i < totalNrRows; i++)
            {
                gameMatrix.Add(null);
            }

            gameMatrix[0] = SetupTopRow();
        }

        public void ClearBalls() => balls.Clear();

        private void DrawAimingBalls(SpriteBatch spriteBatch, col.Vector aim)
        {
            int nrBalls = 10;
            float xStep = aim.X / nrBalls;
            float yStep = aim.Y / nrBalls;
            for (int i = nrBalls; i > 0; i--)
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
                GameState.State = State.GameOver;
                return;
            }

            for (int i = gameMatrix.Count - 1; i > 0; i--)
                gameMatrix[i] = gameMatrix[i - 1];

            GameState.Turn++;
            

            gameMatrix[0] = SetupTopRow();
            if (gameMatrix.Count > 8)
                gameMatrix.RemoveAt(gameMatrix.Count - 1);
            GameState.State = State.Positioning;
            ballFactory.IsFirstBall = true;
            successPlayedThisTurn = false;
        }

        private List<Brick> SetupTopRow()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            int nrColumns = (int)(playArea.Width / brickSize.X);
            var maxValue = GameState.Turn * (nrColumns - 1);
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

                cell.Value = rnd.Next(1, potentialMultiplier + 1) * GameState.Turn;

                cellsToFill.Add(cell);
            }

            return AddBrickRow(cellsToFill, nrColumns);
        }

        private List<Brick> AddBrickRow(List<Cell> cellInfo, int nrColumns)
        {
            var brickRow = new List<Brick>();
            for (int i = 0; i < nrColumns; i++)
            {
                if (cellInfo.Any(c => c.Index == i))
                {
                    var brick = new Brick();
                    brick.Image = brickShading;
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

        private void AimingUpdate()
        {
            var mouseState = inputManager.GetMouseState();
            UpdateAimDirection(mouseState);
            if (mouseState.LeftButton == ButtonState.Released)
            {
                if (aimDirection != null && validAim && aimDirection.Length > 100)
                {
                    ballFactory.InitialVelocity = aimDirection;
                    ballFactory.BallsInFactory = GameState.Turn;
                    ballFactory.IsFirstBall = true;
                    GameState.State = State.Turn;
                }
                else
                    GameState.State = State.Positioning;

                validAim = false;
            }
        }

        private void UpdateAimDirection(MouseState mouseState)
        {
            aimDirection = new col.Vector(
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
                GameState.State = State.Aiming;
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

            GameState.State = State.TurnTransition;
            SetupTransition();
        }

        private void SetupTransition()
        {
            var totalBricks = 0;
            for (int i = gameMatrix.Count - 1; i > -1; i--)
            {
                if (gameMatrix[i] == null)
                    continue;

                foreach (var brick in gameMatrix[i])
                {
                    if (brick != null)
                    {
                        brick.OldYPos = brick.BoundingBox.Center.Y;
                        totalBricks++;
                    }
                }
            }

            if (totalBricks > 0)
                soundManager.PlaySound(Sounds.Slide);
        }

        private void UpdateRows()
        {
            bool allRemoved = true;
            foreach (var row in gameMatrix)
            {
                if (row == null)
                    continue;
                for (int i = 0; i < row.Count; i++)
                {
                    if (row[i] != null && row[i].Counter < 1)
                    {
                        var particleFactory = new ParticleFactory();
                        particleFactory.Position = new Vector2(row[i].BoundingBox.Center.X, row[i].BoundingBox.Center.Y);
                        particleFactory.SetupExplosion();
                        particleFactories.Add(particleFactory);
                        soundManager.PlaySound(Sounds.Popp);
                        row[i] = null;
                        GameState.NrBricksDestroyed++;
                    }
                    else if (row[i] != null)
                        allRemoved = false;
                }
            }

            if (allRemoved && !successPlayedThisTurn)
            {
                soundManager.PlaySound(Sounds.Success);
                successPlayedThisTurn = true;
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
                    gameMatrix);

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
                    var movementStepVector = new col.Vector(ball.Velocity * ball.MovementStep);
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

                        var intersectionObject = CollisionChecks.BrickCollision(ball, gameMatrix[i][j], sourounding);
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
            int nrColumns = (int)(playArea.Width / brickSize.X);

            if (idInner < 0 || idInner > nrColumns - 1)
                return true;
            if (idOuter < 0 ||
                idOuter > gameMatrix.Count - 1 ||
                gameMatrix[idOuter] == null ||
                gameMatrix[idOuter][idInner] == null)
                return false;

            return true;
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

        private void SetupBorders()
        {
            borders = new Dictionary<Walls, ScreenBorder>();

            var sideBorderWidth = (screenSize.Width - playArea.Width) / 2;
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
                        brick.Font = smallFont;
                        brick.Draw(spriteBatch, whitePixel);
                    }
                }
            }
        }

        private class Cell
        {
            public int Value { get; set; } = 0;
            public int Index { get; set; }
        }
    }

    public class StateChangedEventArgs : EventArgs
    {
        public State state;

        public StateChangedEventArgs(State state)
        {
            this.state = state;
        }
    }
}
