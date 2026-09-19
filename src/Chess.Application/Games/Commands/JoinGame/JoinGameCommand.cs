using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.JoinGame
{
    public record JoinGameCommand(Guid GameId, string PlayerId) : IRequest<Result>;
}
