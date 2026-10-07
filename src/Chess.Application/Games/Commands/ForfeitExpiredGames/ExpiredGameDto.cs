namespace Chess.Application.Games.Commands.ForfeitExpiredGames
{
    /// <summary>
    /// Contains information about a game that has expired due to a player's failure to reconnect within the permitted time.    
    /// </summary>
    /// <param name="GameId">The id of the expired game.</param>
    /// <param name="IsWhiteWinner">Indicates whether the white player won the game.</param>
    public record ExpiredGameDto(Guid GameId, bool IsWhiteWinner);
}
