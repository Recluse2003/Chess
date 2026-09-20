namespace Chess.Application.Games.Queries.GetGameCode
{
    public class GameLobbyDto
    {
        public Guid GameId { get; init; }
        public string JoinCode { get; init; } = "";
    }
}