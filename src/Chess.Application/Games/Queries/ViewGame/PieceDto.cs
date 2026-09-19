namespace Chess.Application.Games.Queries.ViewGame
{
    /// <summary>
    /// States the position of a piece on a board, and its type.
    /// </summary>
    public record PieceDto
    {
        public int File { get; init; }
        public int Rank { get; init; }
        public char Piece { get; init; }
    }
}
