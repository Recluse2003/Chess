using Chess.Infrastructure.Persistence;
using Chess.Infrastructure.Persistence.Models;
using Chess.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chess.Tests.Infrastructure.Persistence.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ChessDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ChessDbContext(options);

            _context.Database.EnsureCreated();

            _repository = new UserRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public async Task GetUsernameByIdAsync_ShouldReturnUsername_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-123",
                UserName = "TestPlayer",
                NormalizedUserName = "TESTPLAYER"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            string? result = await _repository.GetUsernameByIdAsync("user-123");

            // Assert
            result.Should().Be("TestPlayer");
        }

        [Fact]
        public async Task GetUsernameByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange and act
            string? result = await _repository.GetUsernameByIdAsync("does-not-exist");

            // Assert
            result.Should().BeNull();
        }
    }
}