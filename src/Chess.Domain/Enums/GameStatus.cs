namespace Chess.Domain.Enums
{
    /// <summary>
    /// Represents the current state of a chess game. 
    /// </summary>
    public enum GameStatus
    {
        WaitingForOpponent,
        NotStarted,
        Active,
        Paused,
        WhiteWin,
        BlackWin,
        Draw,
        Abandoned
    }
}
