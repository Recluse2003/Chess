using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.ForfeitExpiredGames;
using Chess.Application.Games.Queries.GetGamesByUserId;
using Chess.Application.Games.Queries.GetRandomPublicGames;
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

        Task<List<PublicGameDto>> GetRandomPublicGamesAsync(string userId, int count, CancellationToken cancellationToken);

        Task<PagedList<GameDto>> GetGamesByUserIdAsync(string userId, int pageNumber, int pageSize);

        Task<List<ExpiredGameDto>> ForfeitExpiredGamesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Persists all pending changes in the current database context.
        /// </summary>
        Task SaveChangesAsync();
    }
}
