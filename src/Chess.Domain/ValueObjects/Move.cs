namespace Chess.Domain.ValueObjects
{
    /// <summary>
    /// Represents a chess move from one position to another.
    /// </summary>
    public record Move
    {
        public Guid Id { get; }
        public Position From { get; }
        public Position To { get; }
        public char? PromotionPiece { get; }
        public string? FenAfterMove { get; }

        public Move(Guid id, Position from, Position to, char? promotionPiece = null, string? fenAfterMove = null)
        {
            Id = id;
            From = from;
            To = to;
            PromotionPiece = promotionPiece;
            FenAfterMove = fenAfterMove;
        }
    }
}
