using Chess.Application.Common.Results;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Commands.MakeMove
{
    public record MakeMoveCommand(Guid GameId, string PlayerId, Position From, Position To, char? PromotionPiece) : IRequest<Result<MoveResultDto>>;
}
