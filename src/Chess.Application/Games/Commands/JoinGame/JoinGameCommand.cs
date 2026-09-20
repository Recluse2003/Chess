using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.JoinGame
{
    /// <summary>
    /// Contains the details required to join a specified <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="Code">The believed game code linked to the <see cref="ChessGame"/>.</param>
    /// <param name="PlayerId">The identifier of the user attempting to join the <see cref="ChessGame"/>.</param>
    public record JoinGameCommand(string Code, string PlayerId) : IRequest<Result<Guid>>;
}
