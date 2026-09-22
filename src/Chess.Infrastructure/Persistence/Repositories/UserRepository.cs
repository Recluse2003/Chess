using Chess.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChessDbContext _context;

        public UserRepository(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetUsernameByIdAsync(string userId)
        {
            return await _context.Users
                .Where(user => user.Id == userId)
                .Select(user => user.UserName)
                .SingleOrDefaultAsync();
        }
    }
}
