using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Infrastructure.Persistence;
using Chess.Infrastructure.Persistence.Models;
using Chess.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Chess.Application.Interfaces;

namespace Chess.Tests.Infrastructure.Persistence.Repositories
{
    public class ChessGameRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ChessDbContext _context;
        private readonly IChessGameRepository _repository;

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

        private async Task<ApplicationUser> CreateUserAsync(string id)
        {
            var player = new ApplicationUser
            {
                Id = id,
                UserName = id,
                NormalizedUserName = id.ToUpperInvariant(),
            };

            _context.Users.Add(player);
            await _context.SaveChangesAsync();

            return player;
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
        {
            var whitePlayer = await CreateUserAsync("white-player");
            var blackPlayer = await CreateUserAsync("black-player");

            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                whitePlayer.Id,
                FenConverterService.StartingPositionFen);

            game.Join(blackPlayer.Id);
            game.Start(whitePlayer.Id);

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Act
            ChessGame? result = await _repository.GetByIdAsync(game.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(game.Id);
            result.WhitePlayerId.Should().Be(whitePlayer.Id);
            result.BlackPlayerId.Should().Be(blackPlayer.Id);
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
        public async Task CreateAsync_ShouldPersistGame_WhenUsed()
        {
            var whitePlayer = await CreateUserAsync("white-player");
            var blackPlayer = await CreateUserAsync("black-player");

            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                whitePlayer.Id,
                FenConverterService.StartingPositionFen);

            game.Join(blackPlayer.Id);
            game.Start(whitePlayer.Id);

            // Act
            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Assert
            ChessGameEntity? entity = await _context.ChessGames.SingleOrDefaultAsync(x => x.Id == game.Id);

            entity.Should().NotBeNull();
            entity!.WhitePlayerId.Should().Be(whitePlayer.Id);
            entity.BlackPlayerId.Should().Be(blackPlayer.Id);
            entity.Status.Should().Be(GameStatus.Active);
        }

        

        [Fact]
        public async Task GetGameIdByCodeAsync_ShouldReturnGameId_WhenCodeExists()
        {
            var whitePlayer = await CreateUserAsync("white-player");

            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                whitePlayer.Id,
                FenConverterService.StartingPositionFen);

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            GameCodeEntity? codeEntity = await _context.GameCodes.SingleOrDefaultAsync(x => x.ChessGameId == game.Id);

            codeEntity.Should().NotBeNull();

            // Act
            Guid? result = await _repository.GetGameIdByCodeAsync(codeEntity!.Code);

            // Assert
            result.Should().Be(game.Id);
        }

        [Fact]
        public async Task GetGameIdByCodeAsync_ShouldReturnNull_WhenCodeDoesNotExist()
        {
            // Arrange
            string joinCode = "ABCDE";

            // Act
            Guid? result = await _repository.GetGameIdByCodeAsync(joinCode);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateGameCode_WhenGameIsCreated()
        {
            var whitePlayer = await CreateUserAsync("white-player");

            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                whitePlayer.Id,
                FenConverterService.StartingPositionFen);

            // Act
            string joinCode = await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Assert
            GameCodeEntity? code = await _context.GameCodes.SingleOrDefaultAsync(x => x.ChessGameId == game.Id);

            code.Should().NotBeNull();
            code!.Code.Should().Be(joinCode);
            code.Code.Should().HaveLength(5);
            code.ChessGameId.Should().Be(game.Id);
        }

        

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenGameDoesNotExist()
        {
            var whitePlayer = await CreateUserAsync("white-player");

            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                whitePlayer.Id,
                FenConverterService.StartingPositionFen);

            // Act
            Func<Task> act = async () => await _repository.UpdateAsync(game);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Game not found.");
        }

        [Fact]
        public async Task DeleteUnstartedGamesOlderThanAsync_RemovesOldWaitingForOpponentGame()
        {
            // Arrange
            DateTime cutoffTime = DateTime.UtcNow;

            var game = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                CreatedAt = cutoffTime.AddMinutes(-10),
                Status = GameStatus.WaitingForOpponent
            };

            _context.ChessGames.Add(game);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteUnstartedGamesOlderThanAsync(cutoffTime.AddMinutes(-5), CancellationToken.None);

            // Assert
            ChessGameEntity? result = await _context.ChessGames.SingleOrDefaultAsync(g => g.Id == game.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUnstartedGamesOlderThanAsync_RemovesOldNotStartedGame()
        {
            // Arrange
            DateTime cutoffTime = DateTime.UtcNow;

            var game = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                CreatedAt = cutoffTime.AddMinutes(-10),
                Status = GameStatus.NotStarted
            };

            _context.ChessGames.Add(game);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteUnstartedGamesOlderThanAsync(cutoffTime.AddMinutes(-5), CancellationToken.None);

            ChessGameEntity? result = await _context.ChessGames.SingleOrDefaultAsync(g => g.Id == game.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUnstartedGamesOlderThanAsync_DoesNotRemoveNewGame()
        {
            // Arrange
            DateTime cutoffTime = DateTime.UtcNow;

            var game = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                CreatedAt = cutoffTime.AddMinutes(10),
                Status = GameStatus.WaitingForOpponent
            };

            _context.ChessGames.Add(game);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteUnstartedGamesOlderThanAsync(cutoffTime.AddMinutes(-5), CancellationToken.None);

            // Assert
            ChessGameEntity? result = await _context.ChessGames.SingleOrDefaultAsync(g => g.Id == game.Id);

            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(GameStatus.Active)]
        [InlineData(GameStatus.Draw)]
        [InlineData(GameStatus.WhiteWin)]
        [InlineData(GameStatus.BlackWin)]
        [InlineData(GameStatus.Abandoned)]
        [InlineData(GameStatus.Paused)]
        public async Task DeleteUnstartedGamesOlderThanAsync_DoesNotRemoveOldGameWithOtherStatus(GameStatus status)
        {
            // Arrange
            DateTime cutoffTime = DateTime.UtcNow;

            var game = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                CreatedAt = cutoffTime.AddMinutes(-10),
                Status = status
            };

            _context.ChessGames.Add(game);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteUnstartedGamesOlderThanAsync(cutoffTime.AddMinutes(-5), CancellationToken.None);

            // Assert
            ChessGameEntity? result = await _context.ChessGames.SingleOrDefaultAsync(g => g.Id == game.Id);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldReturnActiveGame()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");

            var game = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = GameStatus.Active
            };

            _context.ChessGames.Add(game);
            await _context.SaveChangesAsync();

            // Act
            Guid? result = await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            result.Should().Be(game.Id);
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldReturnNewestActiveGame()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");

            var olderGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                Status = GameStatus.Active
            };

            var newerGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = GameStatus.Active
            };

            _context.ChessGames.AddRange(olderGame, newerGame);
            await _context.SaveChangesAsync();

            // Act
            Guid? result = await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            result.Should().Be(newerGame.Id);
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldAbandonOlderActiveGames()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");

            var oldestGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                Status = GameStatus.Active
            };

            var olderGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                Status = GameStatus.Active
            };

            var newestGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = GameStatus.Active
            };

            _context.ChessGames.AddRange(oldestGame, olderGame, newestGame);
            await _context.SaveChangesAsync();

            // Act
            await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            ChessGameEntity? result = await _context.ChessGames.SingleOrDefaultAsync(g => g.Id == olderGame.Id);

            result.Should().NotBeNull();
            result!.Status.Should().Be(GameStatus.Abandoned);
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldKeepNewestGameActive()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");

            var olderGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                Status = GameStatus.Active
            };

            var newestGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = GameStatus.Active
            };

            _context.ChessGames.AddRange(olderGame, newestGame);
            await _context.SaveChangesAsync();

            // Act
            await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            ChessGameEntity? result = await _context.ChessGames.SingleOrDefaultAsync(g => g.Id == newestGame.Id);

            result.Should().NotBeNull();
            result!.Status.Should().Be(GameStatus.Active);
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldNotModifyOtherUsersGames()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");
            var blackPlayer = await CreateUserAsync("black-player");

            var userGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                Status = GameStatus.Active
            };

            var newestUserGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = GameStatus.Active
            };

            var otherUserGame1 = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = blackPlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                Status = GameStatus.Active
            };

            var otherUserGame2 = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = blackPlayer.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = GameStatus.Active
            };

            _context.ChessGames.AddRange(
                userGame,
                newestUserGame,
                otherUserGame1,
                otherUserGame2);

            await _context.SaveChangesAsync();

            // Act
            await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            otherUserGame1.Status.Should().Be(GameStatus.Active);
            otherUserGame2.Status.Should().Be(GameStatus.Active);
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldIgnoreNonActiveGames()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");
            
            var abandonedGame = new ChessGameEntity
            {
                InitialFen = FenConverterService.StartingPositionFen,
                Fen = FenConverterService.StartingPositionFen,
                WhitePlayerId = whitePlayer.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                Status = GameStatus.Abandoned
            };

            _context.ChessGames.Add(abandonedGame);
            await _context.SaveChangesAsync();

            // Act
            Guid? result = await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveGameByUserIdAsync_ShouldReturnNull_WhenUserHasNoActiveGames()
        {
            // Arrange
            var whitePlayer = await CreateUserAsync("white-player");

            // Act
            Guid? result = await _repository.GetActiveGameByUserIdAsync(whitePlayer.Id);

            // Assert
            result.Should().BeNull();
        }
    }
}
