using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Infrastructure.Persistence;
using Chess.Infrastructure.Persistence.Models;
using Chess.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chess.Tests.Infrastructure.Persistence.Repositories
{
    public class GameCodeRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ChessDbContext _context;
        private readonly GameCodeRepository _repository;

        public GameCodeRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ChessDbContext(options);

            _context.Database.EnsureCreated();

            _repository = new GameCodeRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private async Task<Guid> CreateGameWithCodeAsync(string code)
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGameEntity
            {
                Id = gameId,
                WhitePlayerId = "white-player",
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                Status = GameStatus.WaitingForOpponent
            };

            var gameCode = new GameCodeEntity
            {
                Code = code,
                ChessGameId = gameId
            };

            _context.ChessGames.Add(game);
            _context.GameCodes.Add(gameCode);

            await _context.SaveChangesAsync();

            return gameId;
        }

        [Fact]
        public async Task DeleteCodeByGameIdAsync_ShouldRemoveCode_WhenCodeExists()
        {
            // Arrange
            Guid gameId = await CreateGameWithCodeAsync("ABCDE");

            // Act
            await _repository.DeleteCodeByGameIdAsync(gameId);
            await _context.SaveChangesAsync();

            // Assert
            GameCodeEntity? result = await _context.GameCodes.SingleOrDefaultAsync(x => x.ChessGameId == gameId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteCodeByGameIdAsync_ShouldDoNothing_WhenCodeDoesNotExist()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            // Act
            Func<Task> act = async () =>
            {
                await _repository.DeleteCodeByGameIdAsync(gameId);
                await _context.SaveChangesAsync();
            };

            // Assert
            await act.Should().NotThrowAsync();

            GameCodeEntity? result = await _context.GameCodes.SingleOrDefaultAsync(x => x.ChessGameId == gameId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCodeByGameIdAsync_ShouldReturnCode_WhenGameCodeExists()
        {
            // Arrange
            Guid gameId = await CreateGameWithCodeAsync("ABCDE");

            // Act
            string? result = await _repository.GetCodeByGameIdAsync(gameId);

            // Assert
            result.Should().Be("ABCDE");
        }

        [Fact]
        public async Task GetCodeByGameIdAsync_ShouldReturnNull_WhenGameCodeDoesNotExist()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            // Act
            string? result = await _repository.GetCodeByGameIdAsync(gameId);

            // Assert
            result.Should().BeNull();
        }
    }
}