using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameLobby
{
    /// <summary>
    /// Contains the details required to retrieve the details of a <see cref="ChessGame"/> lobby.
    /// </summary>
    /// <param name="GameId">The id of the <see cref="ChessGame"/>.</param>
    /// <param name="UserId">The id of the user attempting to retrieve the details of <see cref="ChessGame"/>
    /// lobby.</param>
    public record GetGameLobbyQuery(Guid GameId, string UserId) : IRequest<Result<GameLobbyDto>>;
}
