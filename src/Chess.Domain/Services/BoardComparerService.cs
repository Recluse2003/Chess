using Chess.Domain.ValueObjects;

namespace Chess.Domain.Services
{
    public static class BoardComparerService
    {
        /// <summary>
        /// Determines the changes in pieces and their positions required to transform the first <see cref="Board"/> 
        /// state into the second.
        /// </summary>
        /// <param name="first">The original <see cref="Board"/> state.</param>
        /// <param name="second">The target <see cref="Board"/> state.</param>
        /// <returns>
        /// A list containing the <see cref="BoardChange"/>s required to transform the first <see cref="Board"/> 
        /// state into the second.
        /// </returns>
        public static List<BoardChange> Compare(Board first, Board second)
        {
            List<BoardChange> boardChanges = [];

            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    Position position = new(file, rank);

                    char firstBoardPiece = first.GetPiece(position);
                    char secondBoardPiece = second.GetPiece(position);

                    if (firstBoardPiece != secondBoardPiece)
                    {
                        char? piece = secondBoardPiece == '.' ? null : secondBoardPiece;

                        boardChanges.Add(new BoardChange(file, rank, piece)); 
                    }

                }
            }

            return boardChanges;
        }
    }
}
