namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    /// <summary>
    /// Contains the information required to display a public game that is available to join.
    /// </summary>
    public record PublicGameDto
    {
        public string Code { get; init; } = string.Empty; 
        public string WhitePlayerUsername { get; init; } = string.Empty;
    }
}
