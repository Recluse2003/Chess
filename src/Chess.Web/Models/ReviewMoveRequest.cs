namespace Chess.Web.Models
{
    public class ReviewMoveRequest
    {
        public Guid GameId { get; set; }
        public int CurrentMoveNumber { get; set; }
        public int TargetMoveNumber { get; set; }
    }
}
