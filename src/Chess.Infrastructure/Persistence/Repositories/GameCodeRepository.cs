using Chess.Application.Interfaces;
using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories
{
    public class GameCodeRepository : IGameCodeRepository
    {
        private readonly ChessDbContext _context;

        public GameCodeRepository(ChessDbContext context)
        {
            _context = context;
        }

        public async Task DeleteCodeByGameIdAsync(Guid gameId)
        {
            GameCodeEntity? entity = await _context.GameCodes
                .SingleOrDefaultAsync(c => c.ChessGameId == gameId);

            if (entity == null)
                return;

            _context.GameCodes.Remove(entity);
        }

        public async Task<string?> GetCodeByGameIdAsync(Guid gameId)
        {
            return await _context.GameCodes
                .Where(c => c.ChessGameId == gameId)
                .Select(c => c.Code)
                .SingleOrDefaultAsync();
        }
    }
}
