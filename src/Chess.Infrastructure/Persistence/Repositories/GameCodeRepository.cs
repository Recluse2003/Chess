using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories
{
    /// <summary> 
    /// Provides persistence operations for <see cref="GameCodeEntity">. 
    /// </summary>
    public class GameCodeRepository : IGameCodeRepository
    {
        private readonly ChessDbContext _context;

        public GameCodeRepository(ChessDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Removes the <see cref="GameCodeEntity"> associated with a <see cref="ChessGame">.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"> whose <see cref="GameCodeEntity">
        /// is being removed.</param>
        public async Task DeleteCodeByGameIdAsync(Guid gameId)
        {
            GameCodeEntity? entity = await _context.GameCodes
                .SingleOrDefaultAsync(c => c.ChessGameId == gameId);

            if (entity == null)
                return;

            _context.GameCodes.Remove(entity);
        }

        /// <summary>
        /// Retrieves the <see cref="GameCodeEntity"> associated with a <see cref="ChessGame">.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame">.</param>
        /// <returns>
        /// The join code connected to the specified <see cref="ChessGame">. Returns <see langword="null"/> if
        /// no <see cref="GameCodeEntity"/> exists.
        /// </returns>
        public async Task<string?> GetCodeByGameIdAsync(Guid gameId)
        {
            return await _context.GameCodes
                .Where(c => c.ChessGameId == gameId)
                .Select(c => c.Code)
                .SingleOrDefaultAsync();
        }
    }
}
