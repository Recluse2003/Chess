using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.LeaveLobby;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class LeaveLobbyCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock = new();
        private readonly LeaveLobbyCommandHandler _sut;

        public LeaveLobbyCommandHandlerTests()
        {
            _sut = new LeaveLobbyCommandHandler(_gameRepositoryMock.Object);
        }

        private static ChessGame CreateLobby(bool opponentJoined = false)
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            if (opponentJoined)
                game.Join("black-player");

            return game;
        }

        private void SetupGame(ChessGame? game)
        {
            _gameRepositoryMock.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(game);
        }

        [Fact]
        public async Task Handle_WhenGameDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();
            SetupGame(null);

            LeaveLobbyCommand command = new(gameId, "white-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.GameNotFound");

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenHostLeavesWaitingLobby_ShouldDeleteGameAndReturnTrue()
        {
            // Arrange
            ChessGame game = CreateLobby();

            SetupGame(game);

            LeaveLobbyCommand command = new(game.Id, "white-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(game.Id),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenHostLeavesLobbyWithOpponent_ShouldDeleteGameAndReturnTrue()
        {
            // Arrange
            ChessGame game = CreateLobby(opponentJoined: true);

            SetupGame(game);

            LeaveLobbyCommand command = new(game.Id, "white-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(game.Id),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenBlackPlayerLeavesLobby_ShouldUpdateGameAndReturnFalse()
        {
            // Arrange
            ChessGame game = CreateLobby(opponentJoined: true);

            SetupGame(game);

            LeaveLobbyCommand command = new(game.Id, "black-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();

            game.BlackPlayerId.Should().BeNull();
            game.Status.Should().Be(GameStatus.WaitingForOpponent);

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(game),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenNonPlayerAttemptsToLeave_ShouldReturnUnauthorized()
        {
            // Arrange
            ChessGame game = CreateLobby(opponentJoined: true);

            SetupGame(game);

            LeaveLobbyCommand command = new(game.Id, "another-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.NotPlayer");

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenGameIsActive_ShouldReturnConflict()
        {
            // Arrange
            ChessGame game = CreateLobby(opponentJoined: true);
            game.Start("white-player");

            SetupGame(game);

            LeaveLobbyCommand command = new(game.Id, "black-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.AlreadyActive");

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenGameIsPaused_ShouldReturnConflict()
        {
            // Arrange
            ChessGame game = CreateLobby(opponentJoined: true);
            game.Start("white-player");
            game.Pause("white-player");

            SetupGame(game);

            LeaveLobbyCommand command = new(game.Id, "black-player");

            // Act
            Result<bool> result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.AlreadyActive");

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.DeleteByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }
    }
}