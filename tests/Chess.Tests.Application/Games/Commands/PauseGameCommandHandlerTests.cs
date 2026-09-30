using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.PauseGame;
using Chess.Application.Games.Commands.ResumeGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class PauseGameCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly PauseGameCommandHandler _handler;

        public PauseGameCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _handler = new PauseGameCommandHandler(_gameRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var command = new PauseGameCommand(gameId, "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Id.Should().Be("Games.NotFound");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAPlayer()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(gameId, "white-player", FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new PauseGameCommand(gameId, "not-a-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Id.Should().Be("Games.NotPlayer");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenGameIsNotActive()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(gameId, "white-player", FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var command = new PauseGameCommand(gameId, "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Id.Should().Be("Games.NotActive");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldPauseGame_WhenGameIsActive()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(gameId, "white-player", FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new PauseGameCommand(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(game),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
