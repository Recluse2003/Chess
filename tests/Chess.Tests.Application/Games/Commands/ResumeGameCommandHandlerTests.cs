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
    public class ResumeGameCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly ResumeGameCommandHandler _handler;

        public ResumeGameCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _handler = new ResumeGameCommandHandler(_gameRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var command = new ResumeGameCommand(gameId, "white-player");

            _gameRepositoryMock.Setup(x => x.GetByIdAsync(gameId)).ReturnsAsync((ChessGame?)null);

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

            var game = new ChessGame(gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            game.Pause("white-player");

            var command = new ResumeGameCommand(gameId, "not-a-player");

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
        public async Task Handle_ShouldReturnConflict_WhenGameIsNotPaused()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(gameId, "white-player", FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new ResumeGameCommand(gameId, "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Id.Should().Be("Games.NotPaused");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldResumeGame_WhenGameIsPaused()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(gameId, "white-player", FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");
            game.Pause("white-player");

            var command = new ResumeGameCommand(gameId, "white-player");

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
