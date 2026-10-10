using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.ReviewGame
{
    /// <summary>
    /// Contains the details required to request the state of a specified <see cref="ChessGame"/> to review.
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/>.</param>
    /// <param name="UserId">Id of the user wanting to review the <see cref="ChessGame"/>. Used to verify the user was a player
    /// of the <see cref="ChessGame"/>.</param>
    public record ReviewGameQuery(Guid GameId, string UserId) : IRequest<Result<ReviewGameDto>>;
}
