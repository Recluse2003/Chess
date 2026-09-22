using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Queries.ViewGame
{
    public class ViewGameQueryHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly ViewGameQueryHandler _handler;

        public ViewGameQueryHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _handler = new ViewGameQueryHandler(
                _gameRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var query = new ViewGameQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result<ViewGameDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.NotFound");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync(It.IsAny<string>()),
                Times.Never);
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

            var query = new ViewGameQuery(
                gameId,
                "not-a-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<ViewGameDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Id.Should().Be("Games.Unauthorized");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnGameState_WhenWhitePlayerViewsGame()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var query = new ViewGameQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("white-player"))
                .ReturnsAsync("Alice");

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("black-player"))
                .ReturnsAsync("Bob");

            Result<ViewGameDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value!.GameId.Should().Be(gameId);
            result.Value.WhitePlayerUsername.Should().Be("Alice");
            result.Value.BlackPlayerUsername.Should().Be("Bob");
            result.Value.IsWhitePlayer.Should().BeTrue();
            result.Value.IsWhiteTurn.Should().BeTrue();
            result.Value.Status.Should().Be(GameStatus.Active);

            result.Value.Pieces.Should().NotBeNull();
            result.Value.Pieces.Should().HaveCount(32);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("white-player"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("black-player"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnGameState_WhenBlackPlayerViewsGame()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            // Make a white move so it becomes Black's turn.
            game.MakeMove(new Move(
                Guid.NewGuid(),
                new Position(4, 1),
                new Position(4, 3)));

            var query = new ViewGameQuery(
                gameId,
                "black-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("white-player"))
                .ReturnsAsync("Alice");

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("black-player"))
                .ReturnsAsync("Bob");

            Result<ViewGameDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value!.GameId.Should().Be(gameId);
            result.Value.WhitePlayerUsername.Should().Be("Alice");
            result.Value.BlackPlayerUsername.Should().Be("Bob");
            result.Value.IsWhitePlayer.Should().BeFalse();
            result.Value.IsWhiteTurn.Should().BeFalse();
            result.Value.Status.Should().Be(GameStatus.Active);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("white-player"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("black-player"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllPiecesWithCorrectPositions()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var query = new ViewGameQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("white-player"))
                .ReturnsAsync("Alice");

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("black-player"))
                .ReturnsAsync("Bob");

            Result<ViewGameDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();

            result.Value!.Pieces.Should().Contain(x =>
                x.File == 0 &&
                x.Rank == 0 &&
                x.Piece == 'R');

            result.Value.Pieces.Should().Contain(x =>
                x.File == 4 &&
                x.Rank == 0 &&
                x.Piece == 'K');

            result.Value.Pieces.Should().Contain(x =>
                x.File == 4 &&
                x.Rank == 1 &&
                x.Piece == 'P');

            result.Value.Pieces.Should().Contain(x =>
                x.File == 0 &&
                x.Rank == 7 &&
                x.Piece == 'r');

            result.Value.Pieces.Should().Contain(x =>
                x.File == 4 &&
                x.Rank == 7 &&
                x.Piece == 'k');

            result.Value.Pieces.Should().Contain(x =>
                x.File == 4 &&
                x.Rank == 6 &&
                x.Piece == 'p');
        }
    }
}