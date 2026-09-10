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
            char target = GetPiece(move.To);

            char[,] clonedSquares = new char[8, 8];

            Array.Copy(_squares, clonedSquares, _squares.Length);

            clonedSquares[move.From.File, move.From.Rank] = Empty;
            clonedSquares[move.To.File, move.To.Rank] = piece;

            Position? enPassantTarget = null;

            // Adds en passant target if a pawn moves two spaces, or performs en passant if pawn moved to EnPassantTarget
            if (piece == 'p' || piece == 'P')
            {
                if (Math.Abs(move.From.Rank - move.To.Rank) == 2)
                {
                    enPassantTarget = IsWhiteTurn
                        ? new Position(move.From.File, move.From.Rank + 1)
                        : new Position(move.From.File, move.From.Rank - 1);
                }
                else if (move.To == EnPassantTarget)
                {
                    int capturedPawnRank = IsWhiteTurn 
                        ? move.To.Rank - 1 
                        : move.To.Rank + 1;

                    clonedSquares[move.To.File, capturedPawnRank] = Empty;
                }
            }

            // Promotes black pawn to specified piece if pawn reaches opposite side of board. 
            if (piece == 'p' && move.To.Rank == 0)
                clonedSquares[move.To.File, move.To.Rank] = move.PromotionPiece ?? 'q';

            // Promotes white pawn to specified piece if pawn reaches opposite side of board. 
            if (piece == 'P' && move.To.Rank == 7)
                clonedSquares[move.To.File, move.To.Rank] = move.PromotionPiece ?? 'Q';

            string castlingRights = CastlingRights;

            // Performs castling and/or removes rights
            if ((piece == 'k' || piece == 'K'))
            {
                int rank = move.From.Rank;

                if (move.To.File == 2 && Math.Abs(move.From.File - move.To.File) == 2)
                {
                    clonedSquares[4, rank] = Empty;
                    clonedSquares[2, rank] = piece;

                    clonedSquares[0, rank] = Empty;
                    clonedSquares[3, rank] = IsWhiteTurn ? 'R' : 'r';
                }
                else if (move.To.File == 6 && Math.Abs(move.From.File - move.To.File) == 2)
                {
                    clonedSquares[4, rank] = Empty;
                    clonedSquares[6, rank] = piece;

                    clonedSquares[7, rank] = Empty;
                    clonedSquares[5, rank] = IsWhiteTurn ? 'R' : 'r';
                }

                // Removes castling rights if king moves
                if (IsWhiteTurn)
                {
                    castlingRights = castlingRights.Replace("K", "");
                    castlingRights = castlingRights.Replace("Q", "");
                }
                else
                {
                    castlingRights = castlingRights.Replace("k", "");
                    castlingRights = castlingRights.Replace("q", "");
                }
            }

            // Removes white queen side castling right if rook moves, or piece captures rook. 
            if ((move.From == new Position(0,0) || move.To == new Position(0, 0)) && castlingRights.Contains('Q'))
                castlingRights = castlingRights.Replace("Q", "");

            // Removes white king side castling right if rook moves, or piece captures rook. 
            if ((move.From == new Position(7, 0) || move.To == new Position(7, 0)) && castlingRights.Contains('K'))
                castlingRights = castlingRights.Replace("K", "");

            // Removes black queen side castling right if rook moves, or piece captures rook. 
            if ((move.From == new Position(0, 7) || move.To == new Position(0, 7)) && castlingRights.Contains('q'))
                castlingRights = castlingRights.Replace("q", "");

            // Removes black king side castling right if rook moves, or piece captures rook. 
            if ((move.From == new Position(7, 7) || move.To == new Position(7, 7)) && castlingRights.Contains('k'))
                castlingRights = castlingRights.Replace("k", "");

            int halfmoveClock = HalfmoveClock;

            if (!(target == Empty) || piece == 'p' || piece == 'P')
                halfmoveClock = 0;
            else
                halfmoveClock++;

            int fullmoveNumber = FullmoveNumber;

            if (!IsWhiteTurn)
                fullmoveNumber++;

            return new Board(
                clonedSquares,
                !IsWhiteTurn,
                castlingRights,
                enPassantTarget,
                halfmoveClock,
                fullmoveNumber
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