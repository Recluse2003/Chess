using Chess.Domain.Entities;

namespace Chess.Web.Pages.Chess
{
    /// <summary>
    /// Represents a move request. Contains the details required to move a piece in a specified <see cref="ChessGame"/>, from one
    /// position to another. Is also used for pawn promotions. 
    /// </summary>
    public class MakeMoveRequest
    {
        public Guid GameId { get; set; }
        public int FromFile { get; init; }
        public int FromRank { get; init; }
        public int ToFile { get; init; }
        public int ToRank { get; init; }
        public char? PromotionPiece { get; internal set; }
    }
}
