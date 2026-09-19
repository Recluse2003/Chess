using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;


namespace Chess.Application.Games.Queries.ViewGame
{
    /// <summary>
    /// Contains the details required to request the current state of a specified <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">The identifier of the <see cref="ChessGame"/>.</param>
    /// <param name="PlayerId">The identifier of the player attempting to request the current state of the game.</param>
    public record ViewGameQuery(Guid GameId, string PlayerId) : IRequest<Result<ViewGameDto>>;
}
