using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.ValueObjects
{
    public record Move
    {
        public Position From { get; }
        public Position To { get; }
        public char? PromotionPiece { get; }
        public string? Fen { get; }

        public Move(Position from, Position to, char? promotionPiece = null, string? fen = null)
        {
            From = from;
            To = to;
            PromotionPiece = promotionPiece;
            Fen = fen;
        }
    }
}
