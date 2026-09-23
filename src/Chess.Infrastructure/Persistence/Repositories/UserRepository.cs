using Chess.Application.Interfaces;
using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Provides persistence operations for <see cref="ApplicationUser"/>s.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ChessDbContext _context;

        public UserRepository(ChessDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the username associated with a <see cref="ApplicationUser"/>'s unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the <see cref="ApplicationUser"/>.</param>
        /// <returns>
        /// The username associated with the specified <see cref="ApplicationUser"/>, or 
        /// <see langword="null"/> if the user does not exist.
        /// </returns>
        public async Task<string?> GetUsernameByIdAsync(string userId)
        {
            return await _context.Users
                .Where(user => user.Id == userId)
                .Select(user => user.UserName)
                .SingleOrDefaultAsync();
        }
    }
}
