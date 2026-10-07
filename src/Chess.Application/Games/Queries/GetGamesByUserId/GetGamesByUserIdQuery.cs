using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Queries.GetGamesByUserId
{
    /// <summary>
    /// Represents a request to retrieve a paginated list of games associated with a user.
    /// </summary>
    /// <param name="UserId">The id of the user whose games are to be retrieved</param>
    /// <param name="PageNumber">The page number to retrieve.</param>
    /// <param name="PageSize">The number of games to include per page.</param>
    public record GetGamesByUserIdQuery(string UserId, int PageNumber, int PageSize) : IRequest<Result<PagedList<GameDto>>>;
}
