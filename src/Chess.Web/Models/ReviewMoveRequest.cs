using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;

namespace Chess.Web.Models
{
    /// <summary>
    /// Represents a review move request. Contains the details required to review the state of a <see cref="ChessGame"/> 
    /// after a specified <see cref="Move"/>.
    /// </summary>
    public class ReviewMoveRequest
    {
        public Guid GameId { get; set; }
        public int CurrentMoveNumber { get; set; }
        public int TargetMoveNumber { get; set; }
    }
}
