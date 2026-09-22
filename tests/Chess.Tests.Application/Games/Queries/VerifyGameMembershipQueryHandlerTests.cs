using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.VerifyGameMembershipQuery;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Queries
{
    public class VerifyGameMembershipQueryHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly VerifyGameMembershipQueryHandler _handler;

        public VerifyGameMembershipQueryHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();

            _handler = new VerifyGameMembershipQueryHandler(
                _gameRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var query = new VerifyGameMembershipQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.GameNotFound");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotPlayer()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var query = new VerifyGameMembershipQuery(
                gameId,
                "not-a-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Id.Should().Be("Games.NotPlayer");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserIsWhitePlayer()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var query = new VerifyGameMembershipQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserIsBlackPlayer()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var query = new VerifyGameMembershipQuery(
                gameId,
                "black-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);
        }
    }
}