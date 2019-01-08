using System.Diagnostics;
using static BallBreaker.HelperObjects.Enums;

namespace BallBreaker.Managers
{
    public static class GameState
    {
        public static int Turn { get; set; } = 1;
        public static int HighScore { get; set; } = 1;
        public static string PlayerName { get; set; } = string.Empty;
        public static string HighScorePlayer { get; set; } = string.Empty;
        public static State State { get; set; }
        public static int NrBricksDestroyed { get; set; } = 0;
        public static Stopwatch Stopwatch { get; set; }
    }
}
