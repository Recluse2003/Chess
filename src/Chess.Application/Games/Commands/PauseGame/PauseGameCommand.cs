using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.PauseGame
{
    public record PauseGameCommand(Guid GameId, string UserId) : IRequest<Result>;
}
