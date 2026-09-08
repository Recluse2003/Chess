using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using System.ComponentModel;

namespace Chess.Domain.Services
{
    public class ChessRulesService
    {
        public bool IsMoveLegal(ChessGame chessGame, Move move)
        {
            throw new NotImplementedException();
        }

        public List<Position> GetLegalMoves(ChessGame chessGame, Position piecePosition)
        {
            var candidateMoves = GetCanidiateMoves(chessGame, piecePosition);

            return RemoveKingCheckMoves(chessGame, piecePosition, candidateMoves);
        }

        public List<Position> GetCanidiateMoves(ChessGame chessGame, Position piecePosition)
        {
            var piece = chessGame.Board.GetPiece(piecePosition);

            var candidateMoves = piece switch
            {
                'p' or 'P' => GetPawnMoves(chessGame, piecePosition),
                'n' or 'N' => GetKnightMoves(chessGame, piecePosition),
                'r' or 'R' => GetRookMoves(chessGame, piecePosition),
                'b' or 'B' => GetBishopMoves(chessGame, piecePosition),
                'q' or 'Q' => GetQueenMoves(chessGame, piecePosition),
                'k' or 'K' => GetKingMoves(chessGame, piecePosition),
                _ => []
            };

            return candidateMoves;
        }

        private List<Position> GetPawnMoves(ChessGame chessGame, Position piecePosition)
        {
            Board board = chessGame.Board;
            char piece = board.GetPiece(piecePosition);

            bool isWhitePiece = char.IsUpper(piece);
            int direction = isWhitePiece ? 1 : -1;
            int startingRank = isWhitePiece ? 1 : 6;

            List<Position> candidatePositions = new List<Position>();

            // One square forward
            Position forward = new Position(piecePosition.File, piecePosition.Rank + direction);

            if (board.GetPiece(forward) == '.')
            {
                candidatePositions.Add(forward);

                // Two squares forward
                if (piecePosition.Rank == startingRank)
                {
                    int doubleForwardRank = piecePosition.Rank + direction * 2;
                    Position doubleForward = new Position(piecePosition.File, doubleForwardRank);

                    if (board.GetPiece(doubleForward) == '.')
                        candidatePositions.Add(doubleForward);
                }
            }

            // Capture diagonally left, from white perspective
            Position forwardLeft = new Position(piecePosition.File - 1, piecePosition.Rank + direction);

            if (forwardLeft.File >= 0)
            {
                char forwardLeftValue = board.GetPiece(forwardLeft);
                
                // Verify that pawn can capture diagonally left, or en passant is possible
                if ((forwardLeftValue != '.' && char.IsUpper(forwardLeftValue) != isWhitePiece) || 
                    forwardLeftValue == '.' && board.EnPassantTarget == forwardLeft)
                {
                    candidatePositions.Add(forwardLeft);
                }
            }

            // Capture diagonally right, from white perspective
            Position forwardRight = new Position(piecePosition.File + 1, piecePosition.Rank + direction);

            if (forwardRight.File <= 7)
            {
                char forwardRightValue = board.GetPiece(forwardRight);

                // Verify that pawn can capture diagonally right, or en passant is possible
                if ((forwardRightValue != '.' && char.IsUpper(forwardRightValue) != isWhitePiece) ||
                    forwardRightValue == '.' && board.EnPassantTarget == forwardRight)
                {
                    candidatePositions.Add(forwardRight);
                }
            }

            return candidatePositions;
        }

        private List<Position> GetKnightMoves(ChessGame chessGame, Position piecePosition)
        {
            var board = chessGame.Board;
            var piece = board.GetPiece(piecePosition);

            var offsets = new List<(int File, int Rank)> { (-2, -1), (-2, +1), (-1, -2), (-1, +2), (+1, -2), (+1, +2), (+2, -1), (+2, +1) };

            var candidatePositions = new List<Position>();

            foreach (var offset in offsets)
            {
                int file = piecePosition.File + offset.File;
                int rank = piecePosition.Rank + offset.Rank;

                if (file < 0 || file > 7 || rank < 0 || rank > 7)
                    continue;

                var newPosition = new Position(file, rank);
                var destination = board.GetPiece(newPosition);

                if (destination == '.' || char.IsUpper(piece) != char.IsUpper(destination))
                    candidatePositions.Add(newPosition);
            }

            return candidatePositions;
        }

        private List<Position> GetRookMoves(ChessGame chessGame, Position piecePosition)
        {
            return GetSlidingMoves(chessGame, piecePosition, new() { (1, 0), (0, 1), (-1, 0), (0, -1) });
        }

        private List<Position> GetBishopMoves(ChessGame chessGame, Position piecePosition)
        {
            return GetSlidingMoves(chessGame, piecePosition, new() { (1, -1), (1, 1), (-1, 1), (-1, -1) });
        }

        private List<Position> GetQueenMoves(ChessGame chessGame, Position piecePosition)
        {
            return GetSlidingMoves(chessGame, piecePosition, new() { (1, 0), (0, 1), (-1, 0), (0, -1), (1, -1), (1, 1), (-1, 1), (-1, -1) });
        }

        private List<Position> GetSlidingMoves(ChessGame chessGame, Position piecePosition, List<(int File, int Rank)> offsets)
        {
            var board = chessGame.Board;
            var piece = board.GetPiece(piecePosition);

            var candidatePositions = new List<Position>();

            foreach (var offset in offsets)
            {
                for (int distance = 1; distance < 8; distance++)
                {
                    int file = piecePosition.File + offset.File * distance;
                    int rank = piecePosition.Rank + offset.Rank * distance;

                    if (file < 0 || file > 7 || rank < 0 || rank > 7)
                        break;

                    var newPosition = new Position(file, rank);
                    var destination = board.GetPiece(newPosition);

                    if (destination == '.')
                    {
                        candidatePositions.Add(newPosition);
                        continue;
                    }

                    if (char.IsUpper(piece) != char.IsUpper(destination))
                        candidatePositions.Add(newPosition);

                    break;
                }
            }

            return candidatePositions;
        }

        private List<Position> GetKingMoves(ChessGame chessGame, Position piecePosition)
        {
            var board = chessGame.Board;
            var piece = board.GetPiece(piecePosition);

            var offsets = new List<(int File, int Rank)> { (1, 0), (0, 1), (-1, 0), (0, -1), (1, -1), (1, 1), (-1, 1), (-1, -1) };

            var candidatePositions = new List<Position>();

            foreach (var offset in offsets)
            {
                int file = piecePosition.File + offset.File;
                int rank = piecePosition.Rank + offset.Rank;

                if (file < 0 || file > 7 || rank < 0 || rank > 7)
                    continue;

                var newPosition = new Position(file, rank);
                var destination = board.GetPiece(newPosition);

                if (char.IsUpper(piece) == char.IsUpper(destination))
                    continue;

                candidatePositions.Add(newPosition);
            }

            return candidatePositions;
        }

        private Position? FindKing(Board board, bool isWhitePiece)
        {
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    var location = board.GetPiece(new Position(file, rank));

                    if (char.ToLower(location).Equals('k') && char.IsUpper(location) == isWhitePiece)
                        return new Position(file, rank);
                }
            }

            return null;
        }

        private List<Position> RemoveKingCheckMoves(ChessGame chessGame, Position piecePosition, List<Position> candidateMoves)
        {
            throw new NotImplementedException();
        }

        public bool IsCheckmate(Board board)
        {
            throw new NotImplementedException();
        }
    }
}
