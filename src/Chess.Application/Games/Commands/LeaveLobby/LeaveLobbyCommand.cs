using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.LeaveLobby
{
    /// <summary>
    /// Contains the details required for a player to leave a <see cref="ChessGame"/> still in the lobby. If the player was the 
    /// host, it removes the game entirely. 
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/> to be leaved.</param>
    /// <param name="UserId">Id of the user attempting to leave the <see cref="ChessGame"/>.</param>
    public record LeaveLobbyCommand(Guid GameId, string UserId) : IRequest<Result<bool>>;
}
