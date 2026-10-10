using Chess.Application.Games.Queries.ViewGame;
using Chess.Domain.Enums;

namespace Chess.Application.Games.Queries.ReviewGame
{
    public class ReviewGameDto
    {
        public Guid GameId { get; init; }
        public string WhitePlayerUsername { get; init; } = string.Empty;
        public string BlackPlayerUsername { get; init; } = string.Empty;
        public bool IsWhitePlayer { get; init; }
        public bool IsWhiteTurn { get; init; }

        public GameStatus Status { get; init; }
        public GameEndReason? EndReason { get; init; }

        public int NumberOfMoves { get; init; }
        public IReadOnlyList<PieceDto> Pieces { get; init; } = [];

    }
}
