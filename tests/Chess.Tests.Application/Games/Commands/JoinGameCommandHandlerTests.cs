using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.JoinGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Exceptions;
using Chess.Domain.Services;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class JoinGameCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly JoinGameCommandHandler _handler;

        public JoinGameCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();

            _handler = new JoinGameCommandHandler(_gameRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenCodeDoesNotExist()
        {
            const string code = "ABCDE";
            const string playerId = "black-player";

            var command = new JoinGameCommand(code, playerId);

            _gameRepositoryMock
                .Setup(x => x.GetGameIdByCodeAsync(code))
                .ReturnsAsync((Guid?)null);

            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.InvalidCode");

            _gameRepositoryMock.Verify(
                x => x.GetGameIdByCodeAsync(code),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            const string code = "ABCDE";
            const string playerId = "black-player";
            Guid gameId = Guid.NewGuid();

            var command = new JoinGameCommand(code, playerId);

            _gameRepositoryMock
                .Setup(x => x.GetGameIdByCodeAsync(code))
                .ReturnsAsync(gameId);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.NotFound");

            _gameRepositoryMock.Verify(
                x => x.GetGameIdByCodeAsync(code),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldJoinGameAndReturnGameId_WhenRequestIsValid()
        {
            const string code = "ABCDE";
            const string whitePlayerId = "white-player";
            const string blackPlayerId = "black-player";

            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                whitePlayerId,
                FenConverterService.StartingPositionFen);

            var command = new JoinGameCommand(code, blackPlayerId);

            _gameRepositoryMock
                .Setup(x => x.GetGameIdByCodeAsync(code))
                .ReturnsAsync(gameId);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _gameRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<ChessGame>()))
                .Returns(Task.CompletedTask);

            _gameRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(gameId);

            game.BlackPlayerId.Should().Be(blackPlayerId);

            _gameRepositoryMock.Verify(
                x => x.GetGameIdByCodeAsync(code),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<ChessGame>(g =>
                    g.Id == gameId &&
                    g.WhitePlayerId == whitePlayerId &&
                    g.BlackPlayerId == blackPlayerId)),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenGameRejectsJoin()
        {
            const string code = "ABCDE";
            const string playerId = "black-player";

            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            // Make the game unable to accept another player.
            game.Join(playerId);

            var command = new JoinGameCommand(code, playerId);

            _gameRepositoryMock
                .Setup(x => x.GetGameIdByCodeAsync(code))
                .ReturnsAsync(gameId);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.JoinFailed");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }
    }
}