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
        WhiteWin,
        BlackWin,
        Draw
    }
}
