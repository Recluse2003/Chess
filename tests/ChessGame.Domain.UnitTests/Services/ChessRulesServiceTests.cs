using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;

namespace Chess.Tests.Domain.Services
{
    public class ChessRulesServiceTests
    {
        private readonly ChessRulesService _rulesService = new();

        // Pawn tests

        [Fact]
        public void GetCandidateMoves_WhitePawn_ShouldMoveOneSquareForward()
        {
            // Arrange: White pawn on e2
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 1));

            // Assert
            moves.Should().Contain(new Position(4, 2));
        }

        [Fact]
        public void GetCandidateMoves_WhitePawn_OnStartingRank_ShouldMoveTwoSquares()
        {
            // Arrange: White pawn on e2
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 1));

            // Assert
            moves.Should().Contain(new Position(4, 2));
            moves.Should().Contain(new Position(4, 3));
        }

        [Fact]
        public void GetCandidateMoves_BlackPawn_ShouldMoveTowardsRankOne()
        {
            // Arrange: Black pawn on e7
            string fen = "4k3/4p3/8/8/8/8/8/4K3 b - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 6));

            // Assert
            moves.Should().Contain(new Position(4, 5));
            moves.Should().Contain(new Position(4, 4));
        }

        [Fact]
        public void GetCandidateMoves_Pawn_ShouldCaptureDiagonally()
        {
            // Arrange: White pawn e4, black pieces on d5 and f5
            string fen = "4k3/8/3r1r2/4P3/8/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 3));

            // Assert
            moves.Should().Contain(new Position(3, 4)); // d5
            moves.Should().Contain(new Position(5, 4)); // f5
            moves.Should().Contain(new Position(4, 4)); // e5
        }

        [Fact]
        public void GetCandidateMoves_Pawn_ShouldNotMoveForwardIntoOccupiedSquare()
        {
            // Arrange: White pawn e2, black rook e3
            string fen = "4k3/8/8/8/4r3/4P3/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 2));

            // Assert
            moves.Should().NotContain(new Position(4, 3));
        }

        // Rook tests

        [Fact]
        public void GetCandidateMoves_Rook_ShouldMoveHorizontallyAndVertically()
        {
            // Arrange: White rook on d4
            string fen = "4k3/8/8/8/3R4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().Contain(new Position(3, 0));
            moves.Should().Contain(new Position(3, 7));
            moves.Should().Contain(new Position(0, 3));
            moves.Should().Contain(new Position(7, 3));
        }

        [Fact]
        public void GetCandidateMoves_Rook_ShouldStopAtFriendlyPiece()
        {
            // Arrange: White rook d4, white pawn d6
            string fen = "4k3/3P4/8/8/3R4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().Contain(new Position(3, 4));
            moves.Should().NotContain(new Position(3, 5));
            moves.Should().NotContain(new Position(3, 6));
        }

        [Fact]
        public void GetCandidateMoves_Rook_ShouldCaptureEnemyPiece()
        {
            // Arrange: White rook d4, black rook d6
            string fen = "4k3/3r4/8/8/3R4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().Contain(new Position(3, 5));
            moves.Should().NotContain(new Position(3, 6));
        }


        // Bishop tests

        [Fact]
        public void GetCandidateMoves_Bishop_ShouldMoveDiagonally()
        {
            // Arrange: Bishop d4
            string fen = "4k3/8/8/8/3B4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().Contain(new Position(0, 0));
            moves.Should().Contain(new Position(6, 6));
            moves.Should().Contain(new Position(0, 6));
            moves.Should().Contain(new Position(6, 0));
        }

        [Fact]
        public void GetCandidateMoves_Bishop_ShouldStopAtFriendlyPiece()
        {
            // Arrange: Bishop d4, white pawn f6
            string fen = "4k3/5P2/8/8/3B4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().Contain(new Position(4, 4));
            moves.Should().NotContain(new Position(5, 5));
        }

        [Fact]
        public void GetCandidateMoves_Bishop_ShouldCaptureEnemyPiece()
        {
            // Arrange: Bishop d4, black pawn f6
            string fen = "4k3/5p2/8/8/3B4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().Contain(new Position(5, 5));
            moves.Should().NotContain(new Position(6, 6));
        }

        // Knight tests

        [Fact]
        public void GetCandidateMoves_Knight_ShouldMoveInLShape()
        {
            // Arrange: Knight d4
            string fen = "4k3/8/8/8/3N4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            var expected = new[]
            {
                new Position(1, 2),
                new Position(1, 4),
                new Position(2, 1),
                new Position(2, 5),
                new Position(4, 1),
                new Position(4, 5),
                new Position(5, 2),
                new Position(5, 4)
            };

            moves.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCandidateMoves_Knight_ShouldNotMoveToFriendlyPiece()
        {
            // Arrange: Knight d4, white pawn e6
            string fen = "4k3/4P3/8/8/3N4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().NotContain(new Position(4, 5));
        }

        [Fact]
        public void GetCandidateMoves_Knight_ShouldBeAbleToJumpOverPieces()
        {
            // Arrange: Knight d4 surrounded by friendly pieces
            string fen = "4k3/2PPP3/2P1P3/8/2PNP3/2P1P3/2PPP3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(3, 3));

            // Assert
            moves.Should().NotBeNull();
        }

        // Queen tests

        [Fact]
        public void GetCandidateMoves_Queen_ShouldMoveLikeRookAndBishop()
        {
            // Arrange: Queen d4
            string fen = "4k3/8/8/8/3Q4/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(
                game,
                new Position(3, 3));

            // Assert

            // Rook directions
            moves.Should().Contain(new Position(3, 7));
            moves.Should().Contain(new Position(7, 3));

            // Bishop directions
            moves.Should().Contain(new Position(7, 7));
            moves.Should().Contain(new Position(0, 0));
        }

        // King tests

        [Fact]
        public void GetCandidateMoves_King_ShouldMoveOneSquareInAnyDirection()
        {
            // Arrange: White king e4
            string fen = "4k3/8/8/8/4K3/8/8/8 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 3));

            // Assert
            var expected = new[]
            {
                new Position(3, 2),
                new Position(4, 2),
                new Position(5, 2),
                new Position(3, 3),
                new Position(5, 3),
                new Position(3, 4),
                new Position(4, 4),
                new Position(5, 4)
            };

            moves.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetLegalMoves_King_ShouldNotMoveIntoCheck()
        {
            // Arrange: White king e1, black rook e8
            string fen = "4r3/8/8/8/8/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetLegalMoves(game, new Position(4, 0));

            // Assert
            moves.Should().NotContain(new Position(4, 1));
        }

        // Pinned Pieces

        [Fact]
        public void GetLegalMoves_ForPinnedBishop_ShouldReturnZeroMoves()
        {
            // Arrange: White King e1, White Bishop e2, Black Rook e8.
            // Moving bishop exposes the king to the rook.
            string fen = "4r3/8/8/8/8/8/4B3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            var pinnedBishopPosition = new Position(4, 1);

            // Act
            var legalMoves = _rulesService.GetLegalMoves(game, pinnedBishopPosition);

            // Assert
            legalMoves.Should().BeEmpty();
        }

        [Fact]
        public void GetLegalMoves_ForPinnedRook_ShouldOnlyAllowMovesThatStayOnPinLine()
        {
            // Arrange: White king e1, rook e2, black rook e8.
            string fen = "4r3/8/8/8/8/8/4R3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var legalMoves = _rulesService.GetLegalMoves(game, new Position(4, 1));

            // Assert
            // Rook may move along the e-file without exposing the king.
            legalMoves.Should().Contain(new Position(4, 2));
            legalMoves.Should().Contain(new Position(4, 3));

            // It must not move sideways.
            legalMoves.Should().NotContain(new Position(3, 1));
            legalMoves.Should().NotContain(new Position(5, 1));
        }

        [Fact]
        public void GetLegalMoves_ForPinnedKnight_ShouldReturnZeroMoves()
        {
            // Arrange: White king e1, knight e2, black rook e8.
            // Knight cannot move along the pin line, so every knight
            // move exposes the king.
            string fen = "4r3/8/8/8/8/8/4N3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var legalMoves = _rulesService.GetLegalMoves(game, new Position(4, 1));

            // Assert
            legalMoves.Should().BeEmpty();
        }

        // King Safety

        [Fact]
        public void GetLegalMoves_ShouldRemoveMoveThatExposesKingToRook()
        {
            // Arrange: White king e1, bishop e2, black rook e8.
            string fen = "4r3/8/8/8/8/8/4B3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetLegalMoves(game, new Position(4, 1));

            // Assert
            moves.Should().BeEmpty();
        }

        [Fact]
        public void GetLegalMoves_ShouldAllowMoveWhenPieceCanBlockCheckLine()
        {
            // Arrange: White king e1, bishop e2, black rook e8.
            // Bishop can theoretically move to f3/g4/h5 etc., but those
            // moves don't block the rook's e-file, so none are legal.
            string fen = "4r3/8/8/8/8/8/4B3/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetLegalMoves(game, new Position(4, 1));

            // Assert
            moves.Should().BeEmpty();
        }

        // Checkmate

        [Fact]
        public void IsCheckmate_ShouldReturnTrue_ForFoolsMate()
        {
            // Arrange: Position after:
            // 1. f3 e5
            // 2. g4 Qh4#
            string fen =
                "rnb1kbnr/" +
                "pppp1ppp/" +
                "8/" +
                "4p3/" +
                "6Pq/" +
                "5P2/" +
                "PPPPP2P/" +
                "RNBQKBNR w KQkq - 0 3";

            var game = CreateGame(fen);

            // Act
            var result = _rulesService.IsCheckmate(game.Board);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsCheckmate_ShouldReturnFalse_WhenKingIsNotInCheck()
        {
            // Arrange
            string fen =
                "4k3/" +
                "8/" +
                "8/" +
                "8/" +
                "8/" +
                "8/" +
                "8/" +
                "4K3 w - - 0 1";

            // Act
            var game = CreateGame(fen);

            var result = _rulesService.IsCheckmate(game.Board);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsCheckmate_ShouldReturnFalse_WhenKingIsInCheckButHasLegalMove()
        {
            // Arrange: White king e1 is checked by rook e8.
            // King can escape sideways.
            string fen = "4r3/8/8/8/8/8/8/3K4 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var result = _rulesService.IsCheckmate(game.Board);

            // Assert
            result.Should().BeFalse();
        }

        // ---------------------------------------------------------
        // EMPTY / INVALID POSITIONS
        // ---------------------------------------------------------

        [Fact]
        public void GetCandidateMoves_EmptySquare_ShouldReturnEmpty()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/4K3 w - - 0 1";
            var game = CreateGame(fen);

            // Act
            var moves = _rulesService.GetCanidiateMoves(game, new Position(4, 3));

            // Assert
            moves.Should().BeEmpty();
        }


        // Helper functions

        private static ChessGame CreateGame(string fen)
        {
            return new ChessGame(
                Guid.NewGuid(),
                "white",
                "black",
                fen,
                GameStatus.Active);
        }
    }
}