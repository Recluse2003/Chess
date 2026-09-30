using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using MediatR;

namespace Chess.Application.Games.Commands.EndGame
{
    /// <summary>
    /// Contains the details required to end a game without being done through a move.
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/> being ended.</param>
    /// <param name="UserId">Id of the user that is causing the game to end.</param>
    public record EndGameCommand(Guid GameId, string UserId, GameEndReason Reason) : IRequest<Result>;
}
