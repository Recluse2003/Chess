
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Services
{
    public class SanNotationService
    {
        private readonly ChessRulesService _rules;

        public SanNotationService(ChessRulesService rules)
        {
            _rules = rules;
        }

        public string Generate(Board boardBefore, Move move, Board boardAfter)
        {
            char piece = boardBefore.GetPiece(move.From);

            // Castling.
            if (char.ToLowerInvariant(piece) == 'k' &&  Math.Abs(move.To.File - move.From.File) == 2)
            {
                string castling = move.To.File == 6 ? "O-O" : "O-O-O";
                return AddCheckSuffix(castling, boardAfter);
            }

            bool isPawn = char.ToLowerInvariant(piece) == 'p';

            bool isCapture = boardBefore.GetPiece(move.To) != Board.Empty ||
                (isPawn && move.To == boardBefore.EnPassantTarget && move.From.File != move.To.File);

            string notation = "";

            if (!isPawn)
            {
                notation += char.ToUpperInvariant(piece);
                notation += GetDisambiguation(boardBefore, move, piece);
            }
            else if (isCapture)
            {
                // Pawn captures use the source file.
                notation += (char)('a' + move.From.File);
            }

            if (isCapture)
                notation += "x";

            notation += GetSquareName(move.To);

            // Promotion.
            if (isPawn &&
                move.PromotionPiece is char promotion &&
                (move.To.Rank == 0 || move.To.Rank == 7))
            {
                notation += "=" + char.ToUpperInvariant(promotion);
            }

            return AddCheckSuffix(notation, boardAfter);
        }

        private string AddCheckSuffix(string notation, Board boardAfter)
        {
            if (_rules.IsCheckmate(boardAfter))
                return notation + "#";

            if (_rules.IsKingInCheck(boardAfter))
                return notation + "+";

            return notation;
        }

        private static string GetSquareName(Position position)
        {
            return $"{(char)('a' + position.File)}{position.Rank + 1}";
        }

        private string GetDisambiguation(Board board, Move move, char movingPiece)
        {
            List<Position> alternatives = GetPossibleSourceSquares(board, move.To, movingPiece)
                .Where(position => position != move.From)
                .Where(position =>
                {
                    char candidatePiece = board.GetPiece(position);

                    // The candidate must be the same piece type and colour.
                    if (candidatePiece == Board.Empty ||
                        char.ToLowerInvariant(candidatePiece) != char.ToLowerInvariant(movingPiece) ||
                        char.IsUpper(candidatePiece) != char.IsUpper(movingPiece))
                    {
                        return false;
                    }

                    // The candidate must be able to legally reach the destination.
                    return _rules.GetLegalMoves(board, position).Contains(move.To);
                })
                .ToList();

            if (alternatives.Count == 0)
                return "";

            bool sharesFile = alternatives.Any(position => position.File == move.From.File);

            bool sharesRank = alternatives.Any(position => position.Rank == move.From.Rank);

            // If no alternative shares the moving piece's file,
            // the file letter is sufficient.
            if (!sharesFile)
                return ((char)('a' + move.From.File)).ToString();

            // Otherwise, if no alternative shares its rank,
            // the rank number is sufficient.
            if (!sharesRank)
                return (move.From.Rank + 1).ToString();

            // Otherwise, both file and rank are required.
            return $"{(char)('a' + move.From.File)}{move.From.Rank + 1}";
        }

        private static IEnumerable<Position> GetPossibleSourceSquares(Board board, Position destination, char piece)
        {
            char pieceType = char.ToLowerInvariant(piece);

            // Knights: check the eight possible source squares.
            if (pieceType == 'n')
            {
                int[,] offsets =
                {
                    { 1, 2 }, { 2, 1 },
                    { 2, -1 }, { 1, -2 },
                    { -1, -2 }, { -2, -1 },
                    { -2, 1 }, { -1, 2 }
                };

                for (int i = 0; i < offsets.GetLength(0); i++)
                {
                    Position candidate = new(destination.File + offsets[i, 0], destination.Rank + offsets[i, 1]);

                    if (IsOnBoard(candidate))
                        yield return candidate;
                }

                yield break;
            }

            // Pawns: check the two possible diagonal capture squares.
            if (pieceType == 'p')
            {
                int sourceRank = destination.Rank +
                    (char.IsUpper(piece) ? -1 : 1);

                foreach (int fileOffset in new[] { -1, 1 })
                {
                    Position candidate = new(destination.File + fileOffset, sourceRank);

                    if (IsOnBoard(candidate))
                        yield return candidate;
                }

                yield break;
            }

            if (pieceType is 'r' or 'b' or 'q')
            {
                (int File, int Rank)[] directions =
                {
                    (1, 0), (-1, 0),
                    (0, 1), (0, -1),
                    (1, 1), (1, -1),
                    (-1, 1), (-1, -1)
                };

                foreach (var direction in directions)
                {
                    bool diagonal =
                        direction.File != 0 && direction.Rank != 0;

                    if (pieceType == 'r' && diagonal)
                        continue;

                    if (pieceType == 'b' && !diagonal)
                        continue;

                    int file = destination.File + direction.File;
                    int rank = destination.Rank + direction.Rank;

                    while (file >= 0 && file < 8 &&
                           rank >= 0 && rank < 8)
                    {
                        Position candidate = new(file, rank);

                        // Include the first occupied square, then stop:
                        // pieces cannot move through other pieces.
                        if (board.GetPiece(candidate) != Board.Empty)
                        {
                            yield return candidate;
                            break;
                        }

                        yield return candidate;

                        file += direction.File;
                        rank += direction.Rank;
                    }
                }
            }
        }

        private static bool IsOnBoard(Position position)
        {
            return position.File >= 0 && position.File < 8 &&
                   position.Rank >= 0 && position.Rank < 8;
        }
    }
}
