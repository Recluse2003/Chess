using Chess.Domain.Entities;

namespace Chess.Application.Games.Commands.CreateGame
{
    /// <summary>
    /// Contains the gameId of the newly created <see cref="ChessGame"/>, and it's linked join code. 
    /// </summary>
    /// <param name="GameId">The newly created identifier of the <see cref="ChessGame"/>.</param>
    /// <param name="JoinCode">The join code linked to the new <see cref="ChessGame"/>, to be provided by 
    /// host to the user they want to join.</param>
    public record CreateGameDto(Guid GameId, string JoinCode);
}
