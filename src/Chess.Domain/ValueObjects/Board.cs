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

        public string CastlingRights { get; }      // "KQkq"
        public string EnPassantTarget { get; }     // "e3" or "-"
        public int HalfmoveClock { get; }
        public int FullmoveNumber { get; }

        public const char Empty = '.';

        public Board(char[,] squares, string castlingRights, string enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            _squares = squares;
            CastlingRights = castlingRights;
            EnPassantTarget = enPassantTarget;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
        }

        public Board ApplyMove(Move Move)
        {
            char piece = GetPiece(Move.From);

            SetPiece(Move.From, Empty);
            SetPiece(Move.To, piece);

            throw new NotImplementedException();
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