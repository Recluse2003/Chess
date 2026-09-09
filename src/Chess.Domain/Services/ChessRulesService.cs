using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using System.ComponentModel;

namespace Chess.Domain.Services
{
    public class ChessRulesService
    {
        public bool IsMoveLegal(Board board, Move move)
        {
            if (board.GetPiece(move.From) == '.')
                return false;

            var legalMoves = GetLegalMoves(board, move.From);

            return legalMoves.Contains(move.To);
        }

        public List<Position> GetLegalMoves(Board board, Position piecePosition)
        {
            var candidateMoves = GetCanidiateMoves(board, piecePosition);

            return RemoveKingCheckMoves(board, piecePosition, candidateMoves);
        }

        public List<Position> GetCanidiateMoves(Board board, Position piecePosition)
        {
            var piece = board.GetPiece(piecePosition);

            var candidateMoves = piece switch
            {
                'p' or 'P' => GetPawnMoves(board, piecePosition),
                'n' or 'N' => GetKnightMoves(board, piecePosition),
                'r' or 'R' => GetRookMoves(board, piecePosition),
                'b' or 'B' => GetBishopMoves(board, piecePosition),
                'q' or 'Q' => GetQueenMoves(board, piecePosition),
                'k' or 'K' => GetKingMoves(board, piecePosition),
                _ => []
            };

            return candidateMoves;
        }

        private List<Position> GetPawnMoves(Board board, Position piecePosition)
        {
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

        private List<Position> GetKnightMoves(Board board, Position piecePosition)
        {
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

        private List<Position> GetRookMoves(Board board, Position piecePosition)
        {
            return GetSlidingMoves(board, piecePosition, new() { (1, 0), (0, 1), (-1, 0), (0, -1) });
        }

        private List<Position> GetBishopMoves(Board board, Position piecePosition)
        {
            return GetSlidingMoves(board, piecePosition, new() { (1, -1), (1, 1), (-1, 1), (-1, -1) });
        }

        private List<Position> GetQueenMoves(Board board, Position piecePosition)
        {
            return GetSlidingMoves(board, piecePosition, new() { (1, 0), (0, 1), (-1, 0), (0, -1), (1, -1), (1, 1), (-1, 1), (-1, -1) });
        }

        private List<Position> GetSlidingMoves(Board board, Position piecePosition, List<(int File, int Rank)> offsets)
        {
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

        private List<Position> GetKingMoves(Board board, Position piecePosition)
        {
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


        private Position FindKing(Board board, bool isWhitePiece)
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

            throw new InvalidOperationException("King not found on board.");
        }

        private List<Position> RemoveKingCheckMoves(Board board, Position piecePosition, List<Position> candidateMoves)
        {
            char piece = board.GetPiece(piecePosition);
            bool isWhitePiece = char.IsUpper(piece);

            var isKing = false;
            Position kingPosition;

            if (piece == 'k' || piece == 'K')
            {
                kingPosition = piecePosition;
                isKing = true;
            }
            else
            {
                kingPosition = FindKing(board, isWhitePiece);
            }

            var legalMoves = new List<Position>();

            foreach (var destination in candidateMoves)
            {
                Board boardWithSimulatedMove = board.ApplyMove(new Move(piecePosition, destination));

                if (isKing)
                    kingPosition = destination;

                if (!IsSquareAttacked(boardWithSimulatedMove, kingPosition, !isWhitePiece))
                    legalMoves.Add(destination);

            }

            return legalMoves;
        }

        private bool IsSquareAttacked(Board board, Position position, bool byWhite)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piecePosition = new Position(file, rank);
                    var piece = board.GetPiece(piecePosition);

                    if (piece == '.')
                        continue;

                    if (char.IsUpper(piece) != byWhite)
                        continue;

                    List<Position> attacks;

                    if (piece == 'P' || piece == 'p')
                    {
                        attacks = GetPawnAttacks(board, piecePosition);
                    }
                    else
                    {
                        attacks = GetCanidiateMoves(board, piecePosition);
                    }

                    if (attacks.Contains(position))
                        return true;
                }
            }

            return false;
        }

        private List<Position> GetPawnAttacks(Board board, Position piecePosition)
        {
            char piece = board.GetPiece(piecePosition);

            bool isWhitePiece = char.IsUpper(piece);
            int direction = isWhitePiece ? 1 : -1;

            var attacks = new List<Position>();

            int attackRank = piecePosition.Rank + direction;

            if (attackRank < 0 || attackRank > 7)
                return attacks;

            if (piecePosition.File > 0)
            { 
                attacks.Add(new Position(piecePosition.File - 1, attackRank));
            }

            if (piecePosition.File < 7)
            {
                attacks.Add(new Position(piecePosition.File + 1, attackRank));
            }

            return attacks;
        }

        public bool IsCheckmate(Board board)
        {
            Position kingPosition = FindKing(board, board.IsWhiteTurn);

            return IsSquareAttacked(board, kingPosition, !board.IsWhiteTurn);
        }
    }
}
