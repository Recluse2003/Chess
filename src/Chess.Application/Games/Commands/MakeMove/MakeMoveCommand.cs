namespace Chess.Application.Games.Commands.MakeMove
{
    public record MakeMoveCommand(Guid GameId, string PlayerId, string From, string To, char? PromotionPiece);
}
