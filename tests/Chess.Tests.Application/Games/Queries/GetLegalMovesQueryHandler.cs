using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.GetLegalMoves;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Queries
{
    public class GetLegalMovesQueryHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly ChessRulesService _rules;
        private readonly GetLegalMovesQueryHandler _handler;

        public GetLegalMovesQueryHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _rules = new ChessRulesService();

            _handler = new GetLegalMovesQueryHandler(
                _gameRepositoryMock.Object,
                _rules);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var query = new GetLegalMovesQuery(
                gameId,
                "white-player",
                new Position(4, 1));

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result<List<LegalMoveDto>> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.NotFound);
            result.Error.Id.Should().Be("Games.NotFound");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenItIsNotPlayersTurn()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var query = new GetLegalMovesQuery(
                gameId,
                "black-player",
                new Position(4, 6));

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<List<LegalMoveDto>> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.NotYourTurn");

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenSquareIsEmpty()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            // e3 is empty in the starting position.
            var query = new GetLegalMovesQuery(
                gameId,
                "white-player",
                new Position(4, 2));

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<List<LegalMoveDto>> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenPieceBelongsToOpponent()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            // Black pawn on e7 while it is White's turn.
            var query = new GetLegalMovesQuery(
                gameId,
                "white-player",
                new Position(4, 6));

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<List<LegalMoveDto>> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnLegalMoves_WhenPieceBelongsToCurrentPlayer()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            // White pawn on e2 can move to e3 or e4.
            var query = new GetLegalMovesQuery(
                gameId,
                "white-player",
                new Position(4, 1));

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<List<LegalMoveDto>> result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value.Should().HaveCount(2);

            result.Value.Should().Contain(x =>
                x.File == 4 &&
                x.Rank == 2);

            result.Value.Should().Contain(x =>
                x.File == 4 &&
                x.Rank == 3);
        }
    }
}