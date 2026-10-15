using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Exceptions;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;


namespace Chess.Domain.UnitTests.Entities
{
    public class ChessGameTests
    {
        private readonly ChessRulesService _rulesService = new();

        private static ChessGame CreateGame(string fen)
        {
            var game = new ChessGame(
                Guid.NewGuid(),
                "white-player",
                fen);

            game.Join("black-player");

            game.Start("white-player");

            return game;
        }

        private static Move CreateHistoricalMove(string fen)
        {
            return new Move(
                Guid.NewGuid(),
                new Position(0, 0),
                new Position(0, 1),
                null,
                fen);
        }

        [Fact]
        public void Constructor_ShouldCreateGameWaitingForOpponent()
        {
            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            Guid id = Guid.NewGuid();

            ChessGame game = new(id, "white-player", fen, true);

            game.Id.Should().Be(id);
            game.WhitePlayerId.Should().Be("white-player");
            game.BlackPlayerId.Should().BeNull();
            game.InitialFen.Should().Be(fen);
            game.IsPrivate.Should().BeTrue();
            game.Status.Should().Be(GameStatus.WaitingForOpponent);
            game.EndReason.Should().BeNull();
            game.MoveHistory.Should().BeEmpty();
            game.Board.Should().NotBeNull();

        }

        [Fact]
        public void Join_WhenLobbyIsWaiting_ShouldAddBlackPlayer()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Join("black-player");

            game.BlackPlayerId.Should().Be("black-player");
            game.Status.Should().Be(GameStatus.NotStarted);

        }

        [Fact]
        public void Join_WhenLobbyIsNotWaiting_ShouldThrow()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Join("another-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Join_WhenBlackPlayerAlreadyJoined_ShouldThrow()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Join("black-player");

            Action act = () => game.Join("another-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Join_WhenHostAttemptsToJoinOwnGame_ShouldThrow()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Join("white-player");

            act.Should().Throw<GameStateTransitionException>()
                .WithMessage("You cannot join your own game.");

        }

        [Fact]
        public void Leave_WhenBlackPlayerLeavesBeforeGameStarts_ShouldReopenLobby()
        {
            var game = new ChessGame(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            game.Join("black-player");

            game.Leave("black-player");

            game.BlackPlayerId.Should().BeNull();
            game.Status.Should().Be(GameStatus.WaitingForOpponent);

        }

        [Fact]
        public void Leave_WhenWhiteHostAttemptsToLeave_ShouldThrow()
        {
            var game = new ChessGame(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            game.Join("black-player");

            Action act = () => game.Leave("white-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Leave_WhenNonParticipantAttemptsToLeave_ShouldThrow()
        {
            var game = new ChessGame(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            game.Join("black-player");

            Action act = () => game.Leave("another-player");

            act.Should().Throw<NotPlayerException>();

        }

        [Fact]
        public void Leave_WhenGameIsActive_ShouldThrow()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Leave("black-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Start_WhenHostStartsJoinedGame_ShouldActivateGame()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Join("black-player");
            game.Start("white-player");

            game.Status.Should().Be(GameStatus.Active);

        }

        [Fact]
        public void Start_WhenNonHostAttemptsToStart_ShouldThrow()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Join("black-player");

            Action act = () => game.Start("black-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Start_WhenOpponentHasNotJoined_ShouldThrow()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Start("white-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Start_WhenGameHasAlreadyStarted_ShouldThrow()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Start("white-player");

            act.Should().Throw<GameStateTransitionException>();

        }

        [Fact]
        public void Pause_WhenActivePlayerDisconnects_ShouldPauseGameAndSetDeadline()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            DateTime before = DateTime.UtcNow;

            game.Pause("white-player");

            DateTime after = DateTime.UtcNow;

            game.Status.Should().Be(GameStatus.Paused);
            game.DisconnectedPlayerId.Should().Be("white-player");
            game.ReconnectDeadline.Should().NotBeNull();
            game.ReconnectDeadline!.Value.Should()
                .BeOnOrAfter(before.AddSeconds(60))
                .And.BeOnOrBefore(after.AddSeconds(60));

        }

        [Fact]
        public void Pause_WhenGameIsNotActive_ShouldThrow()
        {
            ChessGame game = new(Guid.NewGuid(), "white-player", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Pause("white-player");

            act.Should().Throw<GameNotActiveException>();

        }

        [Fact]
        public void Pause_WhenNonParticipantDisconnects_ShouldThrow()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Pause("another-player");

            act.Should().Throw<NotPlayerException>();

        }

        [Fact]
        public void Resume_WhenPausedPlayerReconnects_ShouldRestoreActiveState()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Pause("white-player");

            game.Resume("white-player");

            game.Status.Should().Be(GameStatus.Active);
            game.DisconnectedPlayerId.Should().BeNull();
            game.ReconnectDeadline.Should().BeNull();

        }

        [Fact]
        public void Resume_WhenGameIsNotPaused_ShouldThrow()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Action act = () => game.Resume("white-player");

            act.Should().Throw<GameNotPausedException>();

        }

        [Fact]
        public void Resume_WhenNonParticipantAttemptsToReconnect_ShouldThrow()
        {
            ChessGame game = CreateGame("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Pause("white-player");

            Action act = () => game.Resume("another-player");

            act.Should().Throw<NotPlayerException>();

        }

        [Fact]
        public void CheckGameState_WhenBlackIsCheckmated_ShouldSetWhiteWin()
        {
            // Black king on h8, white queen on g7, white king on f6.
            string fen = "7k/6Q1/5K2/8/8/8/8/8 b - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.WhiteWin);
            game.EndReason.Should().Be(GameEndReason.Checkmate);
        }

        [Fact]
        public void CheckGameState_WhenWhiteIsCheckmated_ShouldSetBlackWin()
        {
            // White king on h1, black queen on g2, black king on f3.
            string fen = "8/8/8/8/8/5k2/6q1/7K w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.BlackWin);
            game.EndReason.Should().Be(GameEndReason.Checkmate);
        }

        [Fact]
        public void CheckGameState_WhenPositionIsStalemate_ShouldSetDraw()
        {
            // Black king on h8, white queen on f7, white king on g6.
            string fen = "7k/5Q2/6K1/8/8/8/8/8 b - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.Stalemate);
        }

        [Fact]
        public void CheckGameState_KingVsKing_ShouldBeInsufficientMaterial()
        {
            string fen = "4k3/8/8/8/8/8/8/4K3 w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.InsufficientMaterial);
        }

        [Fact]
        public void CheckGameState_BishopVsKing_ShouldBeInsufficientMaterial()
        {
            string fen = "4k3/8/8/8/8/8/2B5/4K3 w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.InsufficientMaterial);
        }

        [Fact]
        public void CheckGameState_KnightVsKing_ShouldBeInsufficientMaterial()
        {
            string fen = "4k3/8/8/8/8/8/2N5/4K3 w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.InsufficientMaterial);
        }

        [Fact]
        public void CheckGameState_SameColourBishops_ShouldBeInsufficientMaterial()
        {
            string fen = "4k3/8/8/8/8/8/2B3b1/4K3 w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.InsufficientMaterial);
        }

        [Fact]
        public void CheckGameState_RookVsKing_ShouldNotBeInsufficientMaterial()
        {
            string fen = "4k3/8/8/8/8/8/2R5/4K3 w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.EndReason.Should().NotBe(GameEndReason.InsufficientMaterial);
        }

        [Fact]
        public void CheckGameState_WhenHalfmoveClockReaches100_ShouldSetDraw()
        {
            string fen = "4k3/8/8/8/8/8/7r/4K3 w - - 100 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.FiftyMoveRule);
        }

        [Fact]
        public void CheckGameState_WhenHalfmoveClockIs99_ShouldNotSetFiftyMoveDraw()
        {
            string fen = "4k3/8/8/8/8/8/8/4K3 w - - 99 1";

            ChessGame game = CreateGame(fen);

            game.CheckGameState(_rulesService);

            game.EndReason.Should().NotBe(GameEndReason.FiftyMoveRule);
        }

        [Fact]
        public void CheckGameState_WhenPositionOccursThreeTimes_ShouldSetDraw()
        {
            string fen = "4k3/8/8/8/8/8/7r/4K3 w - - 0 1";

            ChessGame game = CreateGame(fen);

            game.MoveHistory.Add(CreateHistoricalMove("4k3/8/8/8/8/8/7r/4K3 w - - 0 1"));

            game.MoveHistory.Add(CreateHistoricalMove("4k3/8/8/8/8/8/7r/4K3 w - - 1 1"));

            game.MoveHistory.Add(CreateHistoricalMove("4k3/8/8/8/8/8/7r/4K3 w - - 2 2"));

            game.CheckGameState(_rulesService);

            game.Status.Should().Be(GameStatus.Draw);
            game.EndReason.Should().Be(GameEndReason.ThreefoldRepetition);
        }


    }
}
