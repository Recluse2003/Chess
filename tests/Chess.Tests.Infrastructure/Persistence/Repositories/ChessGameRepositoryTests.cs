using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Infrastructure.Persistence;
using Chess.Infrastructure.Persistence.Models;
using Chess.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Chess.Tests.Infrastructure.Persistence.Repositories
{
    public class ChessGameRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ChessDbContext _context;
        private readonly ChessGameRepository _repository;

        public ChessGameRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ChessDbContext(options);

            _context.Database.EnsureCreated();

            _repository = new ChessGameRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Act
            ChessGame? result = await _repository.GetByIdAsync(game.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(game.Id);
            result.WhitePlayerId.Should().Be("white-player");
            result.BlackPlayerId.Should().Be("black-player");
            result.Status.Should().Be(GameStatus.Active);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenGameDoesNotExist()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            ChessGame? result = await _repository.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldPersistGame()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            // Act
            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Assert
            ChessGameEntity? entity = await _context.ChessGames.SingleOrDefaultAsync(x => x.Id == game.Id);

            entity.Should().NotBeNull();
            entity!.WhitePlayerId.Should().Be("white-player");
            entity.BlackPlayerId.Should().Be("black-player");
            entity.Status.Should().Be(GameStatus.Active);
        }

        [Fact]
        public async Task Update_ShouldPersistChanges()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Act
            game.MakeMove(new Move(Guid.NewGuid(), new Position(4, 1), new Position(4, 3))); 

            await _repository.UpdateAsync(game);
            await _repository.SaveChangesAsync();

            // Assert
            ChessGameEntity? entity = await _context.ChessGames.SingleOrDefaultAsync(x => x.Id == game.Id);

            entity.Should().NotBeNull();
            entity!.Fen.Should().Be("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldRestoreMoveHistory()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            game.MakeMove(new Move(Guid.NewGuid(), new Position(4, 1), new Position(4, 3)));

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Act
            ChessGame? result = await _repository.GetByIdAsync(game.Id);

            // Assert
            result.Should().NotBeNull();
            result!.MoveHistory.Should().HaveCount(1);

            result.MoveHistory[0].From.Should().Be(new Position(4, 1));
            result.MoveHistory[0].To.Should().Be(new Position(4, 3));
        }
    }
}
