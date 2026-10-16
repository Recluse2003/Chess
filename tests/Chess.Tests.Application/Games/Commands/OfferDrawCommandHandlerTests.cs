using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.OfferDraw;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class OfferDrawCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock = new();
        private readonly OfferDrawCommandHandler _sut;

        public OfferDrawCommandHandlerTests()
        {
            _sut = new OfferDrawCommandHandler(_gameRepositoryMock.Object);
        }

        private static ChessGame CreateActiveGame()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Join("black-player");
            game.Start("white-player");

            return game;
        }

        private void SetupGame(ChessGame? game)
        {
            _gameRepositoryMock
                .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(game);
        }

        [Fact]
        public async Task Handle_WhenGameDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            Guid gameId = Guid.NewGuid();
            SetupGame(null);

            OfferDrawCommand command = new(gameId, "white-player");

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.GameNotFound");

            VerifyNoPersistence();
        }

        [Theory]
        [InlineData("white-player")]
        [InlineData("black-player")]
        public async Task Handle_WhenPlayerOffersDraw_ShouldSaveOffer(
            string playerId)
        {
            // Arrange
            ChessGame game = CreateActiveGame();
            SetupGame(game);

            OfferDrawCommand command = new(game.Id, playerId);

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            game.DrawOfferedByPlayerId.Should().Be(playerId);

            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(game),
                Times.Once);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRequesterIsNotAPlayer_ShouldReturnUnauthorized()
        {
            // Arrange
            ChessGame game = CreateActiveGame();
            SetupGame(game);

            OfferDrawCommand command = new(game.Id, "another-player");

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.NotPlayer");

            game.DrawOfferedByPlayerId.Should().BeNull();

            VerifyNoPersistence();
        }

        [Fact]
        public async Task Handle_WhenGameIsWaitingForOpponent_ShouldReturnConflict()
        {
            // Arrange
            ChessGame game = new(
                Guid.NewGuid(),
                "white-player",
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            SetupGame(game);

            OfferDrawCommand command = new(game.Id, "white-player");

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.NotActive");

            VerifyNoPersistence();
        }

        [Fact]
        public async Task Handle_WhenGameIsPaused_ShouldReturnConflict()
        {
            // Arrange
            ChessGame game = CreateActiveGame();
            game.Pause("white-player");

            SetupGame(game);

            OfferDrawCommand command = new(game.Id, "black-player");

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.NotActive");

            VerifyNoPersistence();
        }

        [Fact]
        public async Task Handle_WhenGameHasAlreadyStartedButIsNotActive_ShouldReturnConflict()
        {
            // Arrange
            ChessGame game = new(
                Guid.NewGuid(),
                "white-player",
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Join("black-player");

            SetupGame(game);

            OfferDrawCommand command = new(game.Id, "white-player");

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.NotActive");

            VerifyNoPersistence();
        }

        [Fact]
        public async Task Handle_WhenDrawOfferAlreadyExists_ShouldReturnConflict()
        {
            // Arrange
            ChessGame game = CreateActiveGame();
            game.OfferDraw("white-player");

            SetupGame(game);

            OfferDrawCommand command = new(game.Id, "black-player");

            // Act
            Result result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Id.Should().Be("Games.OfferDrawFailed");

            game.DrawOfferedByPlayerId.Should().Be("white-player");

            VerifyNoPersistence();
        }

        private void VerifyNoPersistence()
        {
            _gameRepositoryMock.Verify(
                repository => repository.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);
        }
    }
}