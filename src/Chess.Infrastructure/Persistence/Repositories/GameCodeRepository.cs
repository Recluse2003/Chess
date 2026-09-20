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

        public async Task<Guid?> ConsumeCodeAsync(string code)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var gameCode = await _context.GameCodes.FirstOrDefaultAsync(c => c.Code == code.ToUpper().Trim());

                if (gameCode == null) return null;

                var gameId = gameCode.ChessGameId;

                _context.GameCodes.Remove(gameCode);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return gameId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
