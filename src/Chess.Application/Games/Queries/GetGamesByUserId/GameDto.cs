using Chess.Domain.Entities;
using Chess.Domain.Enums;

namespace Chess.Application.Games.Queries.GetGamesByUserId
{
    /// <summary>
    /// Contains summary information about a <see cref="ChessGame"/> for display in the user's game history.
    /// </summary>
    public record GameDto 
    {
        public Guid Id { get; init; }
        public string OpponentUsername { get; init; } = string.Empty;
        public bool WasWhitePlayer { get; init; }
        public int MoveCount { get; init; }
        public GameStatus Status { get; init; }
        public GameEndReason? EndReason { get; init; }
    }
}
