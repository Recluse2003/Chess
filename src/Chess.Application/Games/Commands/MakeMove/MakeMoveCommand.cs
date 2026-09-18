using Chess.Domain.ValueObjects;

namespace Chess.Application.Games.Commands.MakeMove
{
    public record MakeMoveCommand(Guid GameId, string PlayerId, Position From, Position To, char? PromotionPiece);
}
