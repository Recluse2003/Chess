namespace Chess.Application.Games.Queries.GetGameLobby
{
    /// <summary>
    /// Contains the required details needed for a chess lobby.
    /// </summary>
    public class GameLobbyDto
    {
        public Guid GameId { get; init; }
        public string JoinCode { get; init; } = "";
        public bool IsWhitePlayer { get; init; }
        public string WhitePlayerUsername { get; init; } = null!;
        public string? BlackPlayerUsername { get; init; }
    }
}