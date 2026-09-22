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

        [Fact]
        public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

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
        public async Task AddAsync_ShouldPersistGame_WhenUsed()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

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
        public async Task Update_ShouldPersistChanges_WhenUsed()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

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
        public async Task GetByIdAsync_ShouldRestoreMoveHistory_WhenGameIsRetrieved()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

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

        [Fact]
        public async Task GetGameIdByCodeAsync_ShouldReturnGameId_WhenCodeExists()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            GameCodeEntity? codeEntity = await _context.GameCodes
                .SingleOrDefaultAsync(x => x.ChessGameId == game.Id);

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
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
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
        public async Task UpdateAsync_ShouldPersistNewMoves_WhenUpdated()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            var move = new Move(
                Guid.NewGuid(),
                new Position(4, 1),
                new Position(4, 3));

            game.MakeMove(move);

            // Act
            await _repository.UpdateAsync(game);
            await _repository.SaveChangesAsync();

            // Assert
            MoveEntity? entity = await _context.Moves.SingleOrDefaultAsync(x => x.Id == move.Id);

            entity.Should().NotBeNull();
            entity!.GameId.Should().Be(game.Id);
            entity.MoveNumber.Should().Be(1);
            entity.From.Should().Be("e2");
            entity.To.Should().Be("e4");
            entity.FenAfterMove.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotDuplicateExistingMoves_WhenUpdatedAgain()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var move = new Move(
                Guid.NewGuid(),
                new Position(4, 1),
                new Position(4, 3));

            game.MakeMove(move);

            await _repository.CreateAsync(game);
            await _repository.SaveChangesAsync();

            // Act
            await _repository.UpdateAsync(game);
            await _repository.SaveChangesAsync();

            // Update the same game again without adding another move
            await _repository.UpdateAsync(game);
            await _repository.SaveChangesAsync();

            // Assert
            List<MoveEntity> moves = await _context.Moves
                .Where(x => x.GameId == game.Id)
                .ToListAsync();

            moves.Should().HaveCount(1);
            moves[0].Id.Should().Be(move.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenGameDoesNotExist()
        {
            // Arrange
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                FenConverterService.StartingPositionFen);

            // Act
            Func<Task> act = async () => await _repository.UpdateAsync(game);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Game not found.");
        }
    }
}
