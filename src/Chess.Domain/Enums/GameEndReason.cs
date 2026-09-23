namespace Chess.Domain.Enums
{
    /// <summary>
    /// Represents the reason why a chess game has ended.
    /// </summary>
    public enum GameEndReason
    {
        Checkmate,
        Stalemate,
        ThreefoldRepetition,
        FiftyMoveRule,
        InsufficientMaterial,
        Agreement
    }
}
