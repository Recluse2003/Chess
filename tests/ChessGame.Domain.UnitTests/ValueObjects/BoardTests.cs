using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;

namespace Chess.Domain.UnitTests.ValueObjects
{
    public class BoardTests
    {
        [Fact]
        public void ApplyMove_ShouldMovePieceToDestination()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 3)));

            // Assert
            result.GetPiece(new Position(4, 1)).Should().Be('.');
            result.GetPiece(new Position(4, 3)).Should().Be('P');
        }

        [Fact]
        public void ApplyMove_ShouldNotMutateOriginalBoard()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 3)));

            // Assert
            board.GetPiece(new Position(4, 1)).Should().Be('P');
            board.GetPiece(new Position(4, 3)).Should().Be('.');

            result.GetPiece(new Position(4, 1)).Should().Be('.');
            result.GetPiece(new Position(4, 3)).Should().Be('P');
        }

        [Fact]
        public void ApplyMove_ShouldCapturePieceOnDestination()
        {
            // Arrange
            string fen = "4k3/8/8/8/4r3/3B4/8/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(3, 2), new Position(4, 3)));

            // Assert
            result.GetPiece(new Position(3, 2)).Should().Be('.');
            result.GetPiece(new Position(4, 3)).Should().Be('B');
        }

        [Fact]
        public void ApplyMove_WhiteMove_ShouldChangeTurnToBlack()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 2)));

            // Assert
            result.IsWhiteTurn.Should().BeFalse();
        }

        [Fact]
        public void ApplyMove_WhitePawnDoubleMove_ShouldSetEnPassantTarget()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 3)));

            // Assert
            result.EnPassantTarget.Should().Be(new Position(4, 2));
        }

        [Fact]
        public void ApplyMove_BlackPawnDoubleMove_ShouldSetEnPassantTarget()
        {
            // Arrange
            string fen = "4k3/4p3/8/8/8/8/8/4K3 b - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 6), new Position(4, 4)));

            // Assert
            result.EnPassantTarget.Should().Be(new Position(4, 5));
        }

        [Fact]
        public void ApplyMove_NonDoublePawnMove_ShouldClearEnPassantTarget()
        {
            // Arrange
            string fen = "4k3/8/8/8/4P3/8/8/4K3 w - e3 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 3), new Position(4, 4)));

            // Assert
            result.EnPassantTarget.Should().BeNull();
        }

        [Fact]
        public void ApplyMove_WhiteEnPassant_ShouldRemoveCapturedPawn()
        {
            // Arrange
            string fen = "4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 4), new Position(3, 5)));

            // Assert
            result.GetPiece(new Position(4, 4)).Should().Be('.');
            result.GetPiece(new Position(3, 5)).Should().Be('P');
            result.GetPiece(new Position(3, 4)).Should().Be('.');
        }

        [Fact]
        public void ApplyMove_WhitePawnReachingLastRank_ShouldPromote()
        {
            // Arrange
            string fen = "4k3/4P3/8/8/8/8/8/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 6), new Position(4, 7), 'N'));

            // Assert
            result.GetPiece(new Position(4, 7)).Should().Be('N');
        }

        [Fact]
        public void ApplyMove_BlackPawnReachingLastRank_ShouldPromote()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4p3/4K3 b - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 0), 'R'));

            // Assert
            result.GetPiece(new Position(4, 0)).Should().Be('R');
        }

        [Fact]
        public void ApplyMove_WhitePawnPromotionWithoutPiece_ShouldPromoteToQueen()
        {
            // Arrange
            string fen = "4k3/4P3/8/8/8/8/8/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 6), new Position(4, 7)));

            // Assert
            result.GetPiece(new Position(4, 7)).Should().Be('Q');
        }

        [Fact]
        public void ApplyMove_WhiteKingSideCastle_ShouldMoveKingAndRook()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 0), new Position(6, 0)));

            // Assert
            result.GetPiece(new Position(4, 0)).Should().Be('.');
            result.GetPiece(new Position(6, 0)).Should().Be('K');

            result.GetPiece(new Position(7, 0)).Should().Be('.');
            result.GetPiece(new Position(5, 0)).Should().Be('R');
        }

        [Fact]
        public void ApplyMove_WhiteQueenSideCastle_ShouldMoveKingAndRook()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 0), new Position(2, 0)));

            // Assert
            result.GetPiece(new Position(4, 0)).Should().Be('.');
            result.GetPiece(new Position(2, 0)).Should().Be('K');

            result.GetPiece(new Position(0, 0)).Should().Be('.');
            result.GetPiece(new Position(3, 0)).Should().Be('R');
        }

        [Fact]
        public void ApplyMove_WhiteKingMove_ShouldRemoveWhiteCastlingRights()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 0), new Position(4, 1)));

            // Assert
            result.CastlingRights.Should().Be("");
        }

        [Fact]
        public void ApplyMove_WhiteQueenSideRookMove_ShouldRemoveQueenSideCastlingRight()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(0, 0), new Position(0, 1)));

            // Assert
            result.CastlingRights.Should().Be("K");
        }

        [Fact]
        public void ApplyMove_WhiteKingSideRookMove_ShouldRemoveKingSideCastlingRight()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(7, 0), new Position(7, 1)));

            // Assert
            result.CastlingRights.Should().Be("Q");
        }

        [Fact]
        public void ApplyMove_CapturingWhiteQueenSideRook_ShouldRemoveQueenSideCastlingRight()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/1r6/R3K2R b KQ - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(1, 1), new Position(0, 0)));

            // Assert
            result.CastlingRights.Should().Be("K");
        }

        [Fact]
        public void ApplyMove_QuietMove_ShouldIncrementHalfmoveClock()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/8/4K3 w - - 7 10";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 0), new Position(4, 1)));

            // Assert
            result.HalfmoveClock.Should().Be(8);
        }

        [Fact]
        public void ApplyMove_PawnMove_ShouldResetHalfmoveClock()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 7 10";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 2)));

            // Assert
            result.HalfmoveClock.Should().Be(0);
        }

        [Fact]
        public void ApplyMove_Capture_ShouldResetHalfmoveClock()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4p3/4K3 w - - 7 10";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 0), new Position(4, 1)));

            // Assert
            result.HalfmoveClock.Should().Be(0);
        }

        [Fact]
        public void ApplyMove_WhiteMove_ShouldNotIncrementFullmoveNumber()
        {
            // Arrange
            string fen = "4k3/8/8/8/8/8/4P3/4K3 w - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 1), new Position(4, 2)));

            // Assert
            result.FullmoveNumber.Should().Be(1);
        }

        [Fact]
        public void ApplyMove_BlackMove_ShouldIncrementFullmoveNumber()
        {
            // Arrange
            string fen = "4k3/4p3/8/8/8/8/8/4K3 b - - 0 1";
            Board board = FenConverterService.FromFen(fen);

            // Act
            var result = board.ApplyMove(new Move(new Position(4, 6), new Position(4, 5)));

            // Assert
            result.FullmoveNumber.Should().Be(2);
        }
    }
}
