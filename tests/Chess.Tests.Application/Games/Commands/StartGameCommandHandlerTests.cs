using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.StartGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class StartGameCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly Mock<IGameCodeRepository> _codeRepositoryMock;
        private readonly StartGameCommandHandler _handler;

        public StartGameCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _codeRepositoryMock = new Mock<IGameCodeRepository>();

            _handler = new StartGameCommandHandler(
                _gameRepositoryMock.Object,
                _codeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var command = new StartGameCommand(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.NotFound");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _codeRepositoryMock.Verify(
                x => x.DeleteCodeByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldStartGameDeleteCodeAndSave_WhenRequestIsValid()
        {
            Guid gameId = Guid.NewGuid();
            const string whitePlayerId = "white-player";

            var game = new ChessGame(
                gameId,
                whitePlayerId,
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var command = new StartGameCommand(
                gameId,
                whitePlayerId);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _gameRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<ChessGame>()))
                .Returns(Task.CompletedTask);

            _codeRepositoryMock
                .Setup(x => x.DeleteCodeByGameIdAsync(gameId))
                .Returns(Task.CompletedTask);

            _gameRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            Result result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();

            game.Status.Should().Be(GameStatus.Active);

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<ChessGame>(g =>
                    g.Id == gameId &&
                    g.Status == GameStatus.Active)),
                Times.Once);

            _codeRepositoryMock.Verify(
                x => x.DeleteCodeByGameIdAsync(gameId),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenGameCannotBeStarted()
        {
            Guid gameId = Guid.NewGuid();
            const string whitePlayerId = "white-player";

            var game = new ChessGame(
                gameId,
                whitePlayerId,
                FenConverterService.StartingPositionFen);

            // No black player has joined, so Start should fail.
            var command = new StartGameCommand(
                gameId,
                whitePlayerId);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.StartFailed");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _codeRepositoryMock.Verify(
                x => x.DeleteCodeByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenNonPlayerAttemptsToStartGame()
        {
            Guid gameId = Guid.NewGuid();
            const string whitePlayerId = "white-player";

            var game = new ChessGame(
                gameId,
                whitePlayerId,
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var command = new StartGameCommand(
                gameId,
                "not-a-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.StartFailed");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _codeRepositoryMock.Verify(
                x => x.DeleteCodeByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }
    }
}