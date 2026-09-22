using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class CreateGameCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly CreateGameCommandHandler _handler;

        public CreateGameCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _handler = new CreateGameCommandHandler(_gameRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationError_WhenWhitePlayerIdIsEmpty()
        {
            var command = new CreateGameCommand("");

            Result<CreateGameDto> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Validation);
            result.Error.Id.Should().Be("Games.InvalidPlayer");

            _gameRepositoryMock.Verify(
                x => x.CreateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationError_WhenWhitePlayerIdIsWhitespace()
        {
            var command = new CreateGameCommand("   ");

            Result<CreateGameDto> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Validation);
            result.Error.Id.Should().Be("Games.InvalidPlayer");

            _gameRepositoryMock.Verify(
                x => x.CreateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldCreateGameAndReturnDto_WhenCommandIsValid()
        {
            const string playerId = "white-player";
            const string joinCode = "ABCDE";

            var command = new CreateGameCommand(playerId);

            _gameRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<ChessGame>()))
                .ReturnsAsync(joinCode);

            _gameRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            Result<CreateGameDto> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.JoinCode.Should().Be(joinCode);
            result.Value.GameId.Should().NotBeEmpty();

            _gameRepositoryMock.Verify(
                x => x.CreateAsync(It.Is<ChessGame>(game =>
                    game.WhitePlayerId == playerId &&
                    game.Id != Guid.Empty)),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflictError_WhenRepositoryThrowsInvalidOperationException()
        {
            const string playerId = "white-player";
            const string errorMessage = "Failed to generate a unique lobby code.";

            var command = new CreateGameCommand(playerId);

            _gameRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<ChessGame>()))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            Result<CreateGameDto> result =  await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.StateError");
            result.Error.Description.Should().Be(errorMessage);

            _gameRepositoryMock.Verify(
                x => x.CreateAsync(It.IsAny<ChessGame>()),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }
    }
}