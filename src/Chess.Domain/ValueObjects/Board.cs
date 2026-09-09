namespace Chess.Domain.ValueObjects
{
    /// <summary>
    /// Represents the current state of the chess board.
    ///
    /// Piece representation:
    /// Uppercase = White
    /// Lowercase = Black
    ///
    /// P/p = Pawn
    /// N/n = Knight
    /// B/b = Bishop
    /// R/r = Rook
    /// Q/q = Queen
    /// K/k = King
    /// .   = Empty square
    /// </summary>
    public class Board
    {
        private readonly char[,] _squares = new char[8, 8];
        public bool IsWhiteTurn { get; }
        public string CastlingRights { get; }      // "KQkq"
        public Position? EnPassantTarget { get; }     
        public int HalfmoveClock { get; }
        public int FullmoveNumber { get; }

        public const char Empty = '.';

        public Board(char[,] squares, bool isWhiteTurn, string castlingRights, Position? enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            _squares = squares;
            IsWhiteTurn = isWhiteTurn;
            CastlingRights = castlingRights;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        // Returns a new board to ensure value object is immutable. 
        public Board ApplyMove(Move move)
        {
            char piece = GetPiece(move.From);

            char[,] clonedSquares = new char[8, 8];

            Array.Copy(_squares, clonedSquares, _squares.Length);

            clonedSquares[move.From.File, move.From.Rank] = Empty;
            clonedSquares[move.To.File, move.To.Rank] = piece;

          

            return new Board(
                clonedSquares,
                !IsWhiteTurn,
                CastlingRights,
                EnPassantTarget,
                HalfmoveClock,
                FullmoveNumber
            );
        }

        public static char[,] EmptyBoard()
        {
            char[,] emptyBoard = new char[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    emptyBoard[row, col] = Empty;
                }
            }

            return emptyBoard;
        }

        public char GetPiece(Position position)
        {
            return _squares[position.File, position.Rank];
        }

        public void SetPiece(Position position, char piece)
        {
            if (!IsValidPiece(piece))
                throw new ArgumentException("Invalid chess piece.", nameof(piece));

            _squares[position.File, position.Rank] = piece;
        }

        private static bool IsValidPiece(char piece)
        {
            return piece is 'P' or 'N' or 'B' or 'R' or 'Q' or 'K'
                or 'p' or 'n' or 'b' or 'r' or 'q' or 'k'
                or '.';
        }
    }
}