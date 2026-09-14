using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.UnitTests.Entities
{
    public class ChessGameTests
    {
        private readonly ChessRulesService _rulesService = new();

        private static ChessGame CreateGame(string fen, GameStatus status = GameStatus.Active)
        {
            return new ChessGame(
                Guid.NewGuid(),
                "white-player",
                "black-player",
                fen,
                status);
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
