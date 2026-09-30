using Chess.Domain.Entities;

namespace Chess.Application.Interfaces
{
    public interface IChessGameRepository
    {
        Task<ChessGame?> GetByIdAsync(Guid id);
        Task<Guid?> GetGameIdByCodeAsync(string joinCode);
        Task<string> CreateAsync(ChessGame game);
        Task UpdateAsync(ChessGame game);
        Task DeleteUnstartedGamesOlderThanAsync(DateTime cutoffTime, CancellationToken cancellationToken);
        Task<Guid?> GetActiveGameByUserIdAsync(string userId);

        /// <summary>
        /// Persists all pending changes in the current database context.
        /// </summary>
        Task SaveChangesAsync();
    }
}
