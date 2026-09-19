using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.StartGame
{
    /// <summary>
    /// Contains the details required to start a specified <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">Identifier of the <see cref="ChessGame"/>.</param>
    /// <param name="PlayerId">Identifier of the player attempting to start the match.</param>
    public record StartGameCommand(Guid GameId, string PlayerId) : IRequest<Result>;
}
