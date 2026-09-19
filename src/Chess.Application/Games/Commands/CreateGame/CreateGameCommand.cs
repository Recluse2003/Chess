using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.CreateGame
{
    public record CreateGameCommand(string WhitePlayerId) : IRequest<Result<Guid>>;
}
