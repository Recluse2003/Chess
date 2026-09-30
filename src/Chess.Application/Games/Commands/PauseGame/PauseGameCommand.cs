using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.PauseGame
{
    /// <summary>
    /// Contains the details required to pause an active <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/> to be paused.</param>
    /// <param name="UserId">Id of the user attempting to pause the <see cref="ChessGame"/>.</param>
    public record PauseGameCommand(Guid GameId, string UserId) : IRequest<Result>;
}
