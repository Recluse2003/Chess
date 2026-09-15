namespace Chess.Application.Games.MakeMove
{
    public record MakeMoveCommand(Guid GameId, string PlayerId, string From, string To, char? PromotionPiece);
}
