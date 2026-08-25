using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;

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

            throw new NotImplementedException();
        }

        public List<Position> GetCanidiateMoves(ChessGame chessGame, Position piecePosition)
        {
            var piece = chessGame.Board.GetPiece(piecePosition);

            var candidateMoves = piece switch
            {
                'p' or 'P' => GetPawnMoves(chessGame, piecePosition),
                'r' or 'R' => GetRookMoves(chessGame, piecePosition),
                'n' or 'N' => GetKnightMoves(chessGame, piecePosition),
                'b' or 'B' => GetBishopMoves(chessGame, piecePosition),
                'q' or 'Q' => GetQueenMoves(chessGame, piecePosition),
                'k' or 'K' => GetKingMoves(chessGame, piecePosition),
                _ => []
            };

            return candidateMoves;
        }

        private List<Position> GetPawnMoves(ChessGame chessGame, Position piecePosition)
        {
            throw new NotImplementedException();
        }

        private List<Position> GetRookMoves(ChessGame chessGame, Position piecePosition)
        {
            throw new NotImplementedException();
        }

        private List<Position> GetKnightMoves(ChessGame chessGame, Position piecePosition)
        {
            throw new NotImplementedException();
        }

        private List<Position> GetBishopMoves(ChessGame chessGame, Position piecePosition)
        {
            throw new NotImplementedException();
        }

        private List<Position> GetQueenMoves(ChessGame chessGame, Position piecePosition)
        {
            throw new NotImplementedException();
        }

        private List<Position> GetKingMoves(ChessGame chessGame, Position piecePosition)
        {
            throw new NotImplementedException();
        }

        private Position FindKing(Board board, bool IsWhitePiece)
        {
            throw new NotImplementedException();
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
