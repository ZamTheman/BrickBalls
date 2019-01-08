namespace BallBreaker.HelperObjects
{
    public class Enums
    {
        public enum State { Turn, TurnTransition, TransitionNewGameToPosition, Positioning, Aiming, EndOfTurn, GameOver, NewGame };
        public enum Walls { Left, Right, Top };
    }
}
