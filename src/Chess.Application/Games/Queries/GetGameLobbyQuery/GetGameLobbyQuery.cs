using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameCode
{
    public record GetGameLobbyQuery(Guid GameId, string UserId) : IRequest<Result<GameLobbyDto>>;
}
