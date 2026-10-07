using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    /// <summary>
    /// Represents a request to retrieve a random selection of public games that are available to join.
    /// </summary>
    /// <param name="UserId">The id of the current user. Their own games are excluded from the results.</param>
    /// <param name="Count">The maximum number of public games to retrieve.</param>
    public record GetRandomPublicGamesQuery(string UserId, int Count) : IRequest<Result<List<PublicGameDto>>>;
}
