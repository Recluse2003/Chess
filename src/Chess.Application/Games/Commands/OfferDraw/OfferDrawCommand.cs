using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.OfferDraw
{
    /// <summary>
    /// Contains the details required to offer a draw for an active <see cref="ChessGame"/>. 
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/> a draw offer is being sent to.</param>
    /// <param name="UserId">Id of the user that is offering to end the <see cref="ChessGame"/> in a draw.</param>
    public record OfferDrawCommand(Guid GameId, string UserId) : IRequest<Result>;
}
