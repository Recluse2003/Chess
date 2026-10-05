namespace Chess.Application.Games.Commands.ForfeitExpiredGames
{
    public record ExpiredGameDto(Guid GameId, bool IsWhiteWinner);
}
