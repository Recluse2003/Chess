namespace Chess.Application.Interfaces
{
    public interface IGameNotifier
    {
        Task GameEndedByForfeit(Guid gameId, bool isWhiteWinner);
    }
}
