using Chess.Application.Interfaces;
using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;

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
                GameCodeEntity? entity = await _context.GameCodes.FirstOrDefaultAsync(c => c.Code == code.ToUpper().Trim());

                if (entity == null) return null;

                var gameId = entity.ChessGameId;

                _context.GameCodes.Remove(entity);
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

        public async Task<string?> GetCodeByGameIdAsync(Guid gameId)
        {
            return await _context.GameCodes
                .Where(c => c.ChessGameId == gameId)
                .Select(c => c.Code)
                .SingleOrDefaultAsync();
        }
    }
}
