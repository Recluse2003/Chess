using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using FluentAssertions;

namespace Chess.Domain.UnitTests.Services
{
    public class BoardComparerServiceTests
    {
        private static Board CreateBoard(params (string Square, char Piece)[] pieces)
        {
            char[,] squares = new char[8, 8];

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    squares[file, rank] = '.';
                }
            }

            foreach ((string square, char piece) in pieces)
            {
                int file = square[0] - 'a';
                int rank = square[1] - '1';

                squares[file, rank] = piece;
            }

            return new Board(squares, true, "KQkq", null, 0, 1);
        }

        [Fact]
        public void Compare_WhenBoardsAreIdentical_ShouldReturnEmptyList()
        {
            Board first = CreateBoard(
                ("e1", 'K'),
                ("e8", 'k'),
                ("a2", 'P'),
                ("h7", 'p'));

            Board second = CreateBoard(
                ("e1", 'K'),
                ("e8", 'k'),
                ("a2", 'P'),
                ("h7", 'p'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Compare_WhenPieceIsAdded_ShouldReturnAddedPiece()
        {
            Board first = CreateBoard(("e1", 'K'));
            Board second = CreateBoard(("e1", 'K'), ("e4", 'N'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new BoardChange(4, 3, 'N'));
        }

        [Fact]
        public void Compare_WhenPieceIsRemoved_ShouldReturnNullPiece()
        {
            Board first = CreateBoard(("e1", 'K'), ("e4", 'N'));
            Board second = CreateBoard(("e1", 'K'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new BoardChange(4, 3, null));
        }

        [Fact]
        public void Compare_WhenPieceIsReplaced_ShouldReturnNewPiece()
        {
            Board first = CreateBoard(("e1", 'K'), ("e4", 'N'));
            Board second = CreateBoard(("e1", 'K'), ("e4", 'B'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new BoardChange(4, 3, 'B'));
        }

        [Fact]
        public void Compare_WhenMultipleSquaresChange_ShouldReturnAllChanges()
        {
            Board first = CreateBoard(
                ("e1", 'K'),
                ("e8", 'k'),
                ("e2", 'P'),
                ("e7", 'p'));

            Board second = CreateBoard(
                ("e1", 'K'),
                ("e8", 'k'),
                ("e4", 'P'),
                ("e5", 'p'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().HaveCount(4);

            result.Should().BeEquivalentTo(
            [
                new BoardChange(4, 6, null),
                new BoardChange(4, 4, 'p'),
                new BoardChange(4, 3, 'P'),
                new BoardChange(4, 1, null)
            ]);
        }

        [Fact]
        public void Compare_ShouldReturnCoordinatesUsingFileAndRank()
        {
            Board first = CreateBoard();
            Board second = CreateBoard(("a1", 'R'), ("h8", 'k'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().HaveCount(2);

            result.Should().ContainEquivalentOf(new BoardChange(0, 0, 'R'));
            result.Should().ContainEquivalentOf(new BoardChange(7, 7, 'k'));
        }

        [Fact]
        public void Compare_ShouldReturnChangesInDescendingRankThenAscendingFileOrder()
        {
            Board first = CreateBoard();
            Board second = CreateBoard(
                ("a1", 'R'),
                ("h1", 'N'),
                ("a8", 'Q'),
                ("h8", 'k'));

            List<BoardChange> result = BoardComparerService.Compare(first, second);

            result.Should().Equal(
                new BoardChange(0, 7, 'Q'),
                new BoardChange(7, 7, 'k'),
                new BoardChange(0, 0, 'R'),
                new BoardChange(7, 0, 'N'));
        }
    }
}
