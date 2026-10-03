using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.CreateGame
{
    /// <summary>
    /// Contains the details required to create a new <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="WhitePlayerId">The id of the player making the new <see cref="ChessGame"/>. They 
    /// are put as the white player by default.</param>
    /// <param name="IsPrivate">If false, the game is viewable from the public games section.</param>
    public record CreateGameCommand(string WhitePlayerId, bool IsPrivate = false) : IRequest<Result<CreateGameDto>>;
}
