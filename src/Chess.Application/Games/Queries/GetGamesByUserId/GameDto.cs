using Chess.Domain.Enums;

namespace Chess.Application.Games.Queries.GetGamesByUserId
{
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
