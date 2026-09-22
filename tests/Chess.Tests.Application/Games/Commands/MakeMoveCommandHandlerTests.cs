using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Chess.Tests.Application.Games.Commands
{
    public class MakeMoveCommandHandlerTests
    {
        private readonly Mock<IChessGameRepository> _gameRepositoryMock;
        private readonly ChessRulesService _rules;
        private readonly MakeMoveCommandHandler _handler;

        public MakeMoveCommandHandlerTests()
        {
            _gameRepositoryMock = new Mock<IChessGameRepository>();
            _rules = new ChessRulesService();

            _handler = new MakeMoveCommandHandler(
                _gameRepositoryMock.Object,
                _rules);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            Guid gameId = Guid.NewGuid();

            var command = new MakeMoveCommand(
                gameId,
                "white-player",
                new Position(4, 1),
                new Position(4, 3),
                null);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync((ChessGame?)null);

            Result<MoveResultDto> result = await _handler.Handle(command, CancellationToken.None);

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

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenGameIsNotActive()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            var command = new MakeMoveCommand(
                gameId,
                "white-player",
                new Position(4, 1),
                new Position(4, 3),
                null);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<MoveResultDto> result =
                await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.NotActive");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
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

            var command = new MakeMoveCommand(
                gameId,
                "black-player",
                new Position(4, 6),
                new Position(4, 4),
                null);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<MoveResultDto> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Conflict);
            result.Error.Id.Should().Be("Games.NotYourTurn");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationError_WhenMoveIsIllegal()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            // White king cannot move from e1 to e5.
            var command = new MakeMoveCommand(
                gameId,
                "white-player",
                new Position(4, 0),
                new Position(4, 4),
                null);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            Result<MoveResultDto> result =
                await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Type.Should().Be(ErrorType.Validation);
            result.Error.Id.Should().Be("Games.IllegalMove");

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<ChessGame>()),
                Times.Never);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldMakeMoveAndSaveGame_WhenMoveIsLegal()
        {
            Guid gameId = Guid.NewGuid();

            var game = new ChessGame(
                gameId,
                "white-player",
                FenConverterService.StartingPositionFen);

            game.Join("black-player");
            game.Start("white-player");

            var command = new MakeMoveCommand(
                gameId,
                "white-player",
                new Position(4, 1),
                new Position(4, 3),
                null);

            _gameRepositoryMock
                .Setup(x => x.GetByIdAsync(gameId))
                .ReturnsAsync(game);

            _gameRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<ChessGame>()))
                .Returns(Task.CompletedTask);

            _gameRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            Result<MoveResultDto> result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            result.Value!.BoardChanges.Should().NotBeEmpty();
            result.Value.IsWhiteTurn.Should().BeFalse();
            result.Value.Status.Should().Be(GameStatus.Active);
            result.Value.EndReason.Should().BeNull();

            _gameRepositoryMock.Verify(
                x => x.GetByIdAsync(gameId),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<ChessGame>(g =>
                    g.Id == gameId &&
                    g.MoveHistory.Count == 1)),
                Times.Once);

            _gameRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}