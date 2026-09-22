using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.GetGameLobby;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Queries
{
    public class GetGameLobbyQueryHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly Mock<IGameCodeRepository> _codeRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly GetGameLobbyQueryHandler _handler;

        public GetGameLobbyQueryHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _codeRepositoryMock = new Mock<IGameCodeRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _handler = new GetGameLobbyQueryHandler(
                _gameRepositoryMock.Object,
                _codeRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var query = new GetGameLobbyQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result<GameLobbyDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.GameNotFound");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _codeRepositoryMock.Verify(
                x => x.GetCodeByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);

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

            var query = new GetGameLobbyQuery(
                gameId,
                "not-a-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<GameLobbyDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Unauthorized);
            result.Error.Id.Should().Be("Games.NotPlayer");

            _codeRepositoryMock.Verify(
                x => x.GetCodeByGameIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameCodeDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            var query = new GetGameLobbyQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _codeRepositoryMock
                .Setup(x => x.GetCodeByGameIdAsync(gameId))
                .ReturnsAsync((string?)null);

            Result<GameLobbyDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.GameAlreadyStarted");

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnLobbyForWhitePlayer_WhenGameIsWaitingForOpponent()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            var query = new GetGameLobbyQuery(
                gameId,
                "white-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _codeRepositoryMock
                .Setup(x => x.GetCodeByGameIdAsync(gameId))
                .ReturnsAsync("ABCDE");

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("white-player"))
                .ReturnsAsync("Alice");

            Result<GameLobbyDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value!.GameId.Should().Be(gameId);
            result.Value.JoinCode.Should().Be("ABCDE");
            result.Value.IsWhitePlayer.Should().BeTrue();
            result.Value.WhitePlayerUsername.Should().Be("Alice");
            result.Value.BlackPlayerUsername.Should().BeNull();

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("white-player"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("black-player"),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnLobbyForBlackPlayer_WhenBlackPlayerHasJoined()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");

            var query = new GetGameLobbyQuery(
                gameId,
                "black-player");

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _codeRepositoryMock
                .Setup(x => x.GetCodeByGameIdAsync(gameId))
                .ReturnsAsync("ABCDE");

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("white-player"))
                .ReturnsAsync("Alice");

            _userRepositoryMock
                .Setup(x => x.GetUsernameByIdAsync("black-player"))
                .ReturnsAsync("Bob");

            Result<GameLobbyDto> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value!.GameId.Should().Be(gameId);
            result.Value.JoinCode.Should().Be("ABCDE");
            result.Value.IsWhitePlayer.Should().BeFalse();
            result.Value.WhitePlayerUsername.Should().Be("Alice");
            result.Value.BlackPlayerUsername.Should().Be("Bob");

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("white-player"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.GetUsernameByIdAsync("black-player"),
                Times.Once);
        }
    }
}