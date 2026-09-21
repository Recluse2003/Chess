namespace Chess.Application.Games.Queries.GetGameCode
{
    public class GameLobbyDto
    {
        public Guid GameId { get; init; }
        public string JoinCode { get; init; } = "";
        public bool IsWhitePlayer { get; init; }
        public string WhitePlayerUsername { get; init; } = null!;
        public string? BlackPlayerUsername { get; init; }
    }
}