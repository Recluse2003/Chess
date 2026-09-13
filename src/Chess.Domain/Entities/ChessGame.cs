using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Entities
{
    public class ChessGame
    {
        public Guid Id { get; private set; }
        public string WhitePlayerId { get; private set; } = null!;
        public string BlackPlayerId { get; private set; } = null!;
        public Board Board { get; private set; } = null!;
        public GameStatus Status { get; private set; } = GameStatus.Active;
        public GameEndReason? EndReason { get; private set; } = null!;
        public List<Move> MoveHistory { get; private set; } = new();

        public ChessGame(Guid id, string whitePlayerId, string blackPlayerId, string fen, GameStatus gameStatus) 
        { 
            Id = id;
            WhitePlayerId = whitePlayerId;
            BlackPlayerId = blackPlayerId;
            Status = gameStatus;

            Board = FenConverterService.FromFen(fen);
        }

        public void MakeMove(Move move, ChessRulesService rules)
        {
            // Checks if the game is even active
            if (Status != GameStatus.Active)
                throw new InvalidOperationException("Game is not in progress.");

            // Get the piece moving to check turn validity
            char piece = Board.GetPiece(move.From);
            if (char.IsUpper(piece) != Board.IsWhiteTurn)
                throw new InvalidOperationException("It is not this player's turn.");

            // Verify move is legal. this should be moved to application. 
            if (!rules.IsMoveLegal(Board, move))
                throw new InvalidOperationException("Move is not legal.");

            // Delegate the mutation to the board
            Board newBoard = Board.ApplyMove(move);

            Move completedMove = new Move(
                move.From,
                move.To,
                move.PromotionPiece,
                FenConverterService.ToFen(newBoard));

            Board = newBoard;
            MoveHistory.Add(completedMove);
        }

        public void CheckGameState(ChessRulesService rules)
        {
            if (rules.IsCheckmate(Board))
            {
                Status = Board.IsWhiteTurn ? GameStatus.BlackWin : GameStatus.WhiteWin;
                EndReason = GameEndReason.Checkmate;
            }
            else if (rules.IsStalemate(Board))
            {
                Status = GameStatus.Draw;
                EndReason = GameEndReason.Stalemate;
            }
            else if (rules.IsInsufficientMaterial(Board))
            {
                Status = GameStatus.Draw;
                EndReason = GameEndReason.InsufficientMaterial;
            }
            else if (Board.HalfmoveClock >= 100)
            {
                Status = GameStatus.Draw;
                EndReason = GameEndReason.FiftyMoveRule;
            }
            else
            {
                bool isThreefold = MoveHistory
                    .Select(m => GetRepetitionKey(m.Fen))
                    .GroupBy(fen => fen)
                    .Any(group => group.Count() >= 3);

                if (isThreefold)
                {
                    Status = GameStatus.Draw;
                    EndReason = GameEndReason.ThreefoldRepetition;
                }
            }
        }

        public static string GetRepetitionKey(string fen)
        {
            return string.Join(' ', fen.Split(' ').Take(4));
        }
    }
}
