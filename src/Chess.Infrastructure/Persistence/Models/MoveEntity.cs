namespace Chess.Infrastructure.Persistence.Models
{
    public class MoveEntity
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public int MoveNumber { get; set; }
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public char? PromotionPiece { get; set; }
        public string FenAfterMove { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public ChessGameEntity Game { get; set; } = null!;
    }
}
