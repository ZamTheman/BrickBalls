using System;
using System.Collections.Generic;
using BallBreaker.Managers;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static BallBreaker.HelperObjects.Enums;

namespace BallBreaker.Screens
{
    public class GameScreen : IScreen
    {
        private delegate void MenuItemHandler();
        
        private ContentManager content;
        private InputManager inputManager;
        private SoundManager soundManager;
        private GameManager gameManager;
        private GuiManager guiManager;

        private Texture2D background;
        private Rectangle screenSize;

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
            gameManager = new GameManager(
                playAreaWidth,
                playAreaHeight,
                screenSize,
                soundManager,
                inputManager);
            guiManager = new GuiManager(
                screenSize,
                new Dictionary<string, Delegate>
                {
                    { "NEW GAME", new MenuItemHandler(StartNewGame) },
                    { "END TURN", new MenuItemHandler(EndTurnClicked) },
                    { "EXIT", new MenuItemHandler(ExitClicked) }
                },
                inputManager);
            SetupHighScore();
            GameState.Stopwatch = new Stopwatch();
            StartNewGame();
        }

        private void StartNewGame()
        {
            GameState.State = State.NewGame;
            guiManager.UpdateGuiText("TURN", "1");
            guiManager.UpdateGuiText("HIGH SCORE", GameState.HighScorePlayer);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(0, 0, screenSize.Width, screenSize.Height), Color.White);
            gameManager.Draw(spriteBatch);
            guiManager.Draw(spriteBatch);
        }
        
        public void LoadContent()
        {
            soundManager.LoadContent(content);
            gameManager.LoadContent(content);
            guiManager.LoadContent(content);

            // Background
            background = content.Load<Texture2D>("Images/Background");
        }

        public void UnloadContent()
        {   
        }

        public void Update(GameTime gameTime)
        {
            var initialGameState = GameState.State;
            gameManager.Update(gameTime);
            guiManager.Update(gameTime);

            if (GameState.State == State.TransitionNewGameToPosition)
            {
                TransitionNewGameToPosition();
                return;
            }
            
            if (GameState.State == State.GameOver)
                GameState.Stopwatch.Stop();

            if (GameState.State == State.Positioning && initialGameState == State.TurnTransition)
            {
                guiManager.UpdateGuiText("TURN", GameState.Turn.ToString());
                if (GameState.Turn > GameState.HighScore)
                {
                    GameState.HighScore = GameState.Turn;
                    GameState.HighScorePlayer = GameState.PlayerName;
                    guiManager.UpdateGuiText("HIGH SCORE", GameState.HighScorePlayer);
                    SaveHighScore();
                }
            }
        }

        private void TransitionNewGameToPosition()
        {
            gameManager.SetupNewGame();
            GameState.State = State.Positioning;
            GameState.Stopwatch.Reset();
            GameState.Stopwatch.Start();
        }

        private void SetupHighScore()
        {
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BounceMadness");
            Directory.CreateDirectory(folderPath);
            var hs = GetHighScore();

            if (hs == string.Empty)
            {
                GameState.HighScorePlayer = GameState.PlayerName;
                GameState.HighScore = 1;
                return;
            }

            GameState.HighScorePlayer = hs.Split(',')[0];
            GameState.HighScore = int.Parse(hs.Split(',')[1]);
        }

        private void SaveHighScore()
        {
            string filePath = Path.Combine(folderPath, "hs.txt");
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(GameState.PlayerName + "," + GameState.Turn);
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
            gameManager.ClearBalls();
        }

        private void ExitClicked() => Exit.Invoke(null, null);
    }
}
