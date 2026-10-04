namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    public record PublicGameDto
    {
        public string Code { get; init; } = string.Empty; 
        public string WhitePlayerUsername { get; init; } = string.Empty;
    }
}
