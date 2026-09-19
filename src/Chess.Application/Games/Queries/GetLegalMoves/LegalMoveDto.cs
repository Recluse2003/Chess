namespace Chess.Application.Games.Queries.GetLegalMoves
{
    /// <summary>
    /// Contains a legal move that a specified piece can move to.
    /// </summary>
    public record LegalMoveDto
    {
        public int File { get; init; }
        public int Rank { get; init; }
    }
}