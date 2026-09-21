namespace Chess.Web.Pages.Chess
{
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
