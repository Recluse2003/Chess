using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.ResumeGame
{
    /// <summary>
    /// Contains the details required to resume a paused <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/> to be unpaused.</param>
    /// <param name="UserId">Id of the user attempting to unpause the <see cref="ChessGame"/>.</param>
    public record ResumeGameCommand(Guid GameId, string UserId) : IRequest<Result>;
}
