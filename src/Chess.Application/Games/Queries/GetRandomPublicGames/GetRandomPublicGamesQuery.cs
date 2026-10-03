using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    public record GetRandomPublicGamesQuery(string UserId, int Count) : IRequest<Result<List<PublicGameDto>>>;
}
