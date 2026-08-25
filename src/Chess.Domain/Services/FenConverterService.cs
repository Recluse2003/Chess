using Chess.Domain.ValueObjects;
using System.Text;

namespace Chess.Domain.Services
{
    public static class FenConverterService
    {
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

            if (board.FullmoveNumber % 2 != 0)
                fen.Append(" w");
            else
                fen.Append(" b");

            fen.Append($" {board.CastlingRights} {board.EnPassantTarget} {board.HalfmoveClock} {board.FullmoveNumber}");

            return fen.ToString();
        }

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

            bool IsWhiteTurn = gameState[1] == "w";
            int halfmoveClock = int.Parse(gameState[4]);

            return new Board(squares, gameState[1] == "w", gameState[2], gameState[3], int.Parse(gameState[4]), int.Parse(gameState[5]));

        }
    }
}
