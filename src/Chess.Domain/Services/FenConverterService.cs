using Chess.Domain.ValueObjects;
using System.Text;

namespace Chess.Domain.Services
{
    /// <summary>
    /// Provides functionality for converting chess board states to and from Forsyth-Edwards Notation (FEN).
    /// </summary>
    /// <remarks> 
    /// FEN is a notation used to describe a single state of a chess game. It stores the placement of pieces, the 
    /// active player, castling rights, en passant target square, halfmove clock, and fullmove number.  
    /// </remarks>
    public static class FenConverterService
    {
        /// <summary>
        /// Represents the standard starting position of a chess game in FEN.
        /// </summary> 
        public static string StartingPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Converts a <see cref="Board"/> into its FEN representation. 
        /// </summary>
        /// <param name="board">The chess board and associated game-state information to convert.</param>
        /// <returns>
        /// A string containing the complete FEN representation of the board.
        /// </returns>
        public static string ToFen(Board board)
        {
            StringBuilder fen = new StringBuilder();

            for (var rank = 7; rank >= 0; rank--)
            {
                int emptySquares = 0;

                for (var file = 0; file < 8; file++)
                {
                    char piece = board.GetPiece(new Position(file, rank));

                    if (piece == '.')
                    {
                        emptySquares++;
                        continue;
                    }
                    else
                    {
                        if (emptySquares > 0)
                        {
                            fen.Append(emptySquares.ToString());
                            emptySquares = 0;
                        }

                        fen.Append(piece);
                    }
                }

                if (emptySquares > 0)
                    fen.Append(emptySquares);

                if (rank > 0)
                    fen.Append('/');
            }

            if (board.IsWhiteTurn)
                fen.Append(" w");
            else
                fen.Append(" b");

            string enPassantTarget = board.EnPassantTarget?.ToChessNotation() ?? "-";

            fen.Append($" {board.CastlingRights} {enPassantTarget} {board.HalfmoveClock} {board.FullmoveNumber}");

            return fen.ToString();
        }


        /// <summary>
        /// Creates a <see cref="Board"/> from a FEN string.
        /// </summary>
        /// <param name="fen">The FEN string representing the chess position to parse.</param>
        /// <returns>
        /// A <see cref="Board"/> containing the position and game-state information represented by the supplied FEN.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when a FEN rank does not contain exactly eight squares after expanding its piece and empty-square representations.
        /// </exception>
        public static Board FromFen(string fen)
        {
            string[] gameState = fen.Split(' ');

            string[] boardRanks = gameState[0].Split('/');

            char[,] squares = Board.EmptyBoard();

            for (int i = 0; i < 8; i++)
            {
                int rank = 7 - i;
                int file = 0;

                foreach (char character in boardRanks[i])
                {
                    if (char.IsDigit(character))
                    {
                        int emptySquares = character - '0'; // Unicode. 
                        file += emptySquares;
                    }
                    else
                    {
                        squares[file, rank] = character;

                        file++;
                    }
                }

                if (file != 8)
                    throw new ArgumentException("Invalid FEN rank.");
            }

            Position? enPassantTarget = null;

            if (gameState[3] != "-")
                enPassantTarget = Position.FromChessNotation(gameState[3]);

            return new Board(squares, gameState[1] == "w", gameState[2], enPassantTarget, int.Parse(gameState[4]), int.Parse(gameState[5]));
        }
    }
}
