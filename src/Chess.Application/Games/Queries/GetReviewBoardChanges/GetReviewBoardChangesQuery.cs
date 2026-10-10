using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.GetReviewBoardChanges
{
    /// <summary>
    /// Represents a request to retrieve the <see cref="BoardChange"/>s required to transition between two moves
    /// in a <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">
    /// The id of the <see cref="ChessGame"/> being reviewed.
    /// </param>
    /// <param name="UserId">
    /// The id of the user requesting the <see cref="BoardChange"/>s. Used to verify that the user is or was a 
    /// player in the specified <see cref="ChessGame"/>.
    /// </param>
    /// <param name="CurrentMove">
    /// The move position currently displayed by the client.
    /// </param>
    /// <param name="TargetMove">
    /// The move position the client wants to display.
    /// </param>
    public record GetReviewBoardChangesQuery(Guid GameId, string UserId, int CurrentMove, int TargetMove) : IRequest<Result<List<BoardChange>>>;
}
