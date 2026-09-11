using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;

namespace Chess.Domain.UnitTests.Services
{
    public class FenConverterServiceTests
    {
        [Fact]
        public void ToFen_StartOfGame_ReturnsCorrectFen()
        {
            // arrange
            string expectedFen =
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }

        [Fact]
        public void ToFen_RandomMidGame_ReturnsCorrectFen()
        {
            // arrange
            string expectedFen =
                "7N/3np2k/2p3p1/3r3P/2R5/1NP1P3/pK1P4/4b3 w - - 0 1";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }

        [Fact]
        public void FromFen_ShouldPlacePiecesOnCorrectSquares()
        {
            // arrange
            // White Rook on a1 (0,0)
            // Black King on e4 (4,3)
            string fen ="8/8/8/8/4k3/8/8/R3K3 w - - 2 17";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.GetPiece(new Position(0, 0)).Should().Be('R');

            board.GetPiece(new Position(4, 3)).Should().Be('k');

            board.GetPiece(new Position(3, 3)).Should().Be('.');
        }

        [Fact]
        public void FromFen_ShouldCorrectlyExtractCastlingRightsAndEnPassant()
        {
            // arrange
            string fen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b Kq e3 0 1";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.CastlingRights.Should().Be("Kq");
            board.EnPassantTarget.Should().Be(new Position(4, 2));
        }

        [Fact]
        public void FromFen_ShouldConvertEnPassantDashToNull()
        {
            // arrange
            string fen = "8/8/8/8/4k3/8/8/R3K3 w - - 0 1";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.EnPassantTarget.Should().BeNull();
        }

        [Fact]
        public void ToFen_ShouldConvertNullEnPassantTargetToDash()
        {
            // arrange
            string expectedFen = "8/8/8/8/4k3/8/8/R3K3 w - - 0 1";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }

        [Fact]
        public void ToFen_ShouldConvertEnPassantPositionToChessNotation()
        {
            // arrange
            string expectedFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b Kq e3 0 1";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }

        [Fact]
        public void FromFen_ShouldCorrectlyExtractMoveClocks()
        {
            // arrange
            string fen = "8/8/8/8/4k3/8/8/R3K3 w - - 14 28";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.HalfmoveClock.Should().Be(14);
            board.FullmoveNumber.Should().Be(28);
        }

        [Fact]
        public void FromFen_ShouldCorrectlyExtractSideToMove()
        {
            // arrange
            string fen = "8/8/8/8/4k3/8/8/R3K3 b - - 0 17";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.IsWhiteTurn.Should().BeFalse();
        }

        [Fact]
        public void FromFen_ShouldCorrectlyExtractWhiteSideToMove()
        {
            // arrange
            string fen = "8/8/8/8/4k3/8/8/R3K3 w - - 0 17";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.IsWhiteTurn.Should().BeTrue();
        }

        [Fact]
        public void ToFen_ShouldUseBoardSideToMove()
        {
            // arrange
            string expectedFen = "8/8/8/8/4k3/8/8/R3K3 b - - 0 17";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }

        [Theory]
        [InlineData("a1", 0, 0)]
        [InlineData("e4", 4, 3)]
        [InlineData("h8", 7, 7)]
        [InlineData("d6", 3, 5)]
        public void FromFen_EnPassantTarget_ShouldConvertToPosition(string notation, int expectedFile, int expectedRank)
        {
            // arrange
            string fen = $"8/8/8/8/4k3/8/8/R3K3 w - {notation} 0 1";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.EnPassantTarget.Should().Be(new Position(expectedFile, expectedRank));
        }

        [Fact]
        public void ToFen_FromFen_WithEnPassantTarget_ShouldRoundTrip()
        {
            // arrange
            string expectedFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b Kq e3 0 1";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }
    }
}
