using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.JoinGame
{
    /// <summary>
    /// Contains the details required to join a specified <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">The identifier of the <see cref="ChessGame"/> the user is attempting to join.</param>
    /// <param name="PlayerId">The identifier of the user attempting to join the <see cref="ChessGame"/>.</param>
    public record JoinGameCommand(Guid GameId, string PlayerId) : IRequest<Result>;
}
