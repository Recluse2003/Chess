using Chess.Domain.Entities;
using Chess.Domain.Enums;

namespace Chess.Application.Games.Queries.ViewGame
{
    /// <summary>
    /// Contains the current of a specified <see cref="ChessGame"/>, including its id, the players' ids, who turn it is
    /// the current status of the game (Active, waiting for opponents, e.g.), and the pieces on the board and there
    /// positions.
    /// </summary>
    public record ViewGameDto
    {
        public Guid GameId { get; init; }
        public string WhitePlayerId { get; init; } = string.Empty;
        public string? BlackPlayerId { get; init; }
        public bool IsWhitePlayer { get; init; }
        public GameStatus Status { get; init; }
        public string CurrentTurn { get; init; } = string.Empty;
        public IReadOnlyList<PieceDto> Pieces { get; init; } = [];
    }
}
