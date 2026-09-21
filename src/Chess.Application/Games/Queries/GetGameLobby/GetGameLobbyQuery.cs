using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameLobby
{
    public record GetGameLobbyQuery(Guid GameId, string UserId) : IRequest<Result<GameLobbyDto>>;
}
