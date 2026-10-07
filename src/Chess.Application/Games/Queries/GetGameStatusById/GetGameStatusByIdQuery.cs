using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameById
{
    /// <summary>
    /// Contains the details required to retrieve the current status of a specified <see cref="ChessGame"/>
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/>, whose current game status is retrieved.</param>
    public record GetGameStatusByIdQuery(Guid GameId) : IRequest<Result<GameStatus>>;
}
