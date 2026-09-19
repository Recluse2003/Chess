using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.StartGame
{
    public record StartGameCommand(Guid GameId, string PlayerId) : IRequest<Result>;
}
