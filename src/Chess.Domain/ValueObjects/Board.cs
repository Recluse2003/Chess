using Chess.Domain.Enums;

namespace Chess.Domain.ValueObjects
{
    /// <summary>
    /// Represents the current state of the chess board.
    ///
    /// Piece representation: 
    /// Uppercase = White, 
    /// Lowercase = Black,
    ///
    /// P/p = Pawn,
    /// N/n = Knight,
    /// B/b = Bishop,
    /// R/r = Rook,
    /// Q/q = Queen,
    /// K/k = King,
    /// '.'   = Empty square
    /// </summary>
    public class Board
    {
        private readonly char[,] _squares = new char[8, 8];
        public bool IsWhiteTurn { get; }
        public string CastlingRights { get; }      // "KQkq"
        public Position? EnPassantTarget { get; }     
        public int HalfmoveClock { get; }
        public int FullmoveNumber { get; }

        /// <summary> 
        /// Represents an empty square on the chess board. 
        /// </summary>
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

        /// <summary>
        /// Applies a chess move and returns the board state after the move, together with the specific 
        /// changes made to the board. This method also deals with special moves such as castling, en passant 
        /// captures, and pawn promotions. 
        /// </summary>
        /// <param name="move">The move to be applied to the current board.</param>
        /// <returns>
        /// A <see cref="BoardMoveResult"/> containing the new board and the individual board changes 
        /// produced by the move.
        /// </returns>
        /// <remarks>
        /// This method creates a new <see cref="Board"/> rather than modify an existing one. This was done
        /// to ensure <see cref="Board"/> stays true to the definition of a ValueObject. The returned board 
        /// contains the updated turn information, castling rights, en passant state, halfmove clock, and 
        /// fullmove number.
        /// </remarks>
        public BoardMoveResult ApplyMove(Move move)
        {
            List<BoardChange> changes = new();

            char piece = GetPiece(move.From);
            char target = GetPiece(move.To);

            char[,] clonedSquares = new char[8, 8];

            Array.Copy(_squares, clonedSquares, _squares.Length);

            clonedSquares[move.From.File, move.From.Rank] = Empty;
            clonedSquares[move.To.File, move.To.Rank] = piece;

            changes.Add(new BoardChange(move.From.File, move.From.Rank, null));
            

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

                    changes.Add(new BoardChange(move.To.File, capturedPawnRank, null));
                }
            }

            char? pieceToAdd = piece;

            // Promotes black pawn to specified piece if pawn reaches opposite side of board. 
            // Validates the supplied piece is a valid promotion piece.
            if (piece == 'p' && move.To.Rank == 0) 
            {
                char promotionPiece = char.ToLower(move.PromotionPiece ?? 'q');

                if (!IsValidPromotionPiece(promotionPiece))
                    promotionPiece = 'q';

                clonedSquares[move.To.File, move.To.Rank] = promotionPiece; 
                pieceToAdd = promotionPiece; 
            }

            // Promotes white pawn to specified piece if pawn reaches opposite side of board. 
            // Validates the supplied piece is a valid promotion piece.
            if (piece == 'P' && move.To.Rank == 7)
            {
                char promotionPiece = char.ToUpper(move.PromotionPiece ?? 'Q');

                if (!IsValidPromotionPiece(promotionPiece))
                    promotionPiece = 'Q';

                clonedSquares[move.To.File, move.To.Rank] = promotionPiece;
                pieceToAdd = promotionPiece;
            }

            changes.Add(new BoardChange(move.To.File, move.To.Rank, pieceToAdd));

            string castlingRights = CastlingRights;

            // Performs castling and/or removes rights
            if ((piece == 'k' || piece == 'K'))
            {
                int rank = move.From.Rank;

                if (move.To.File == 2 && Math.Abs(move.From.File - move.To.File) == 2)
                {
                    char rook = IsWhiteTurn ? 'R' : 'r';

                    clonedSquares[4, rank] = Empty;
                    clonedSquares[2, rank] = piece;

                    changes.Add(new BoardChange(4, rank, null));
                    changes.Add(new BoardChange(2, rank, piece));

                    clonedSquares[0, rank] = Empty;
                    clonedSquares[3, rank] = rook;

                    changes.Add(new BoardChange(0, rank, null));
                    changes.Add(new BoardChange(3, rank, rook));
                }
                else if (move.To.File == 6 && Math.Abs(move.From.File - move.To.File) == 2)
                {
                    char rook = IsWhiteTurn ? 'R' : 'r';

                    clonedSquares[4, rank] = Empty;
                    clonedSquares[6, rank] = piece;

                    changes.Add(new BoardChange(4, rank, null));
                    changes.Add(new BoardChange(6, rank, piece));

                    clonedSquares[7, rank] = Empty;
                    clonedSquares[5, rank] = rook;

                    changes.Add(new BoardChange(7, rank, null));
                    changes.Add(new BoardChange(5, rank, rook));
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

            Board newBoard = new Board(
                clonedSquares,
                !IsWhiteTurn,
                castlingRights,
                enPassantTarget,
                halfmoveClock,
                fullmoveNumber
            );

            return new BoardMoveResult(
                newBoard,
                changes
            );
        }

        /// <summary>
        /// Creates a new 8-by-8 chess board containing only empty squares. 
        /// </summary>
        /// <returns>
        /// A 2D array in which every square contains '.', which represents empty spaces on the chess board.
        /// </returns>
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

        /// <summary>
        /// Retrieves the piece occupying a specified position on the board. 
        /// </summary>
        /// <param name="position">The position on hte board being inspected.</param>
        /// <returns>
        /// The character that represents the piece at the specified position on the board, or '.' when 
        /// the position is empty. 
        /// </returns>
        public char GetPiece(Position position)
        {
            return _squares[position.File, position.Rank];
        }

        /// <summary>
        /// Determines whether the supplied character represents a valid piece to which a pawn can be 
        /// promoted to.
        /// </summary>
        /// <param name="piece">The piece, represented by a character, being validated.</param>
        /// <returns>
        /// <see langword="true"/> when the piece is a queen, rook, bishop, or knight, otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        private static bool IsValidPromotionPiece(char piece)
        {
            return piece is 'N' or 'B' or 'R' or 'Q' or 'n' or 'b' or 'r' or 'q';
        }
    }
}