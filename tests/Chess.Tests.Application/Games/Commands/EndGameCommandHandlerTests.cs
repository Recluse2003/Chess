using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.EndGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class EndGameCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly EndGameCommandHandler _handler;

        public EndGameCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _handler = new EndGameCommandHandler(_gameRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            var command = new EndGameCommand(Guid.NewGuid(), "user-1", GameEndReason.Resignation);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(command.GameId))
                .ReturnsAsync((ChessGame?)null);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Id.Should().Be("Games.GameNotFound");
        }

        [Fact]
        public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAPlayer()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new EndGameCommand(gameId, "test", GameEndReason.Resignation);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            var handler = new EndGameCommandHandler(_gameRepositoryMock.Object);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Id.Should().Be("Games.NotPlayer");
        }

        [Fact]
        public async Task Handle_ShouldEndGame_WhenUserIsWhitePlayer()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new EndGameCommand(gameId, "white-player", GameEndReason.Resignation);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            var handler = new EndGameCommandHandler(_gameRepositoryMock.Object);

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

        [Fact]
        public async Task Handle_ShouldEndGame_WhenUserIsBlackPlayer()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new EndGameCommand(gameId, "black-player", GameEndReason.Resignation);

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
