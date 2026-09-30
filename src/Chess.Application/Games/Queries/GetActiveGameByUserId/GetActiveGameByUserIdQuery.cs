using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;


namespace Chess.Application.Games.Queries.GetActiveGameByUserId
{
    /// <summary>
    /// Contains the details required to retrieve the id of an active <see cref="ChessGame"/>, 
    /// through id of one of it's players.
    /// </summary>
    /// <param name="UserId">Id of the player.</param>
    public record GetActiveGameByUserIdQuery(string UserId) : IRequest<Result<Guid>>;
}
