using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetActiveLobbyByUserId
{
    /// <summary>
    /// Contains the details required to retrieve the id of a <see cref="ChessGame"/> still in the lobby before it has started, 
    /// through the id of one of it's players.
    /// </summary>
    /// <param name="UserId">Id of the player.</param>
    public record GetActiveLobbyByUserIdQuery(string UserId) : IRequest<Result<Guid>>;
}
