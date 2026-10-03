namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    public record PublicGameDto
    {
        public string Code { get; set; } = string.Empty; 
        public string WhitePlayerUsername { get; set; } = string.Empty;
    }
}
