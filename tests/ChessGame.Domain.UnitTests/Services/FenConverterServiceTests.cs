using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;

namespace ChessGame.Domain.UnitTests.Services
{
    public class FenConverterServiceTests
    {
        public FenConverterServiceTests()
        {

        }

        [Fact]
        public void ToFen_StartOfGame_ReturnsCorrectFen()
        {
            // arrange
            string expectedFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

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
            string expectedFen = "7N/3np2k/2p3p1/3r3P/2R5/1NP1P3/pK1P4/4b3 w - - 0 1";

            // act
            Board board = FenConverterService.FromFen(expectedFen);
            string actualFen = FenConverterService.ToFen(board);

            // assert
            actualFen.Should().Be(expectedFen);
        }

        [Fact]
        public void FromFen_ShouldPlacePiecesOnCorrectSquares()
        {
            // arrange: White Rook on a1 (0,0), Black King on e4 (4,3)
            string fen = "8/8/8/8/4k3/8/8/R3K3 w - - 2 17";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.GetPiece(new Position(0, 0)).Should().Be('R'); // White Rook at a1
            board.GetPiece(new Position(4, 3)).Should().Be('k'); // Black King at e4
            board.GetPiece(new Position(3, 3)).Should().Be('.'); // Empty square at d4
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
            board.EnPassantTarget.Should().Be("e3");
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
        public void FromFen_ShouldHandleEmptyMetadata_WhenUsingDash()
        {
            // arrange
            string fen = "8/8/8/8/4k3/8/8/R3K3 w - - 0 1";

            // act
            Board board = FenConverterService.FromFen(fen);

            // assert
            board.CastlingRights.Should().Be("-");
            board.EnPassantTarget.Should().Be("-");
        }
    }
}
