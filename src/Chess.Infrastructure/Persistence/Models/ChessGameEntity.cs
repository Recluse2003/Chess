using Chess.Domain.Entities;
using Chess.Domain.Enums;

namespace Chess.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Represents a persisted <see cref="ChessGame"/> in the database.
    /// </summary>
    public class ChessGameEntity
    {
        public Guid Id { get; set; }
        public string? WhitePlayerId { get; set; }
        public string? BlackPlayerId { get; set; }
        public string InitialFen { get; set; } = null!;
        public string Fen { get; set; } = null!;
        public GameStatus Status { get; set; }
        public GameEndReason? EndReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MoveEntity> Moves { get; set; } = new List<MoveEntity>();
    }
}
