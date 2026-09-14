using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;

namespace Chess.Infrastructure.Persistence.Models
{
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
