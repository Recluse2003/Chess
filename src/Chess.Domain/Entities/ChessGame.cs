using Chess.Domain.Enums;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Entities
{
    public class ChessGame
    {
        public Guid Id { get; private set; }
        public string WhitePlayerId { get; private set; }
        public string? BlackPlayerId { get; private set; } 
        public string InitialFen { get; private set; }
        public Board Board { get; private set; } = null!;
        public GameStatus Status { get; private set; } = GameStatus.Active;
        public GameEndReason? EndReason { get; private set; } = null!;
        public List<Move> MoveHistory { get; private set; } = new();

        public ChessGame(Guid id, string whitePlayerId, string fen) 
        { 
            Id = id;
            WhitePlayerId = whitePlayerId;

            InitialFen = fen;
            Board = FenConverterService.FromFen(fen);

            Status = GameStatus.WaitingForOpponent;
        }

        public void Join(string playerId)
        {
            if (Status != GameStatus.WaitingForOpponent)
                throw new InvalidOperationException("Game is not accepting players.");

            if (BlackPlayerId != null)
                throw new InvalidOperationException("Game already has an opponent.");

            if (WhitePlayerId == playerId)
                throw new InvalidOperationException("You cannot join your own game.");

            BlackPlayerId = playerId;
            Status = GameStatus.Active;
        }

        public void MakeMove(Move move)
        {
            // Checks if the game is even active
            if (Status != GameStatus.Active)
                throw new InvalidOperationException("Game is not in progress.");

            // Get the piece moving to check turn validity
            char piece = Board.GetPiece(move.From);
            if (char.IsUpper(piece) != Board.IsWhiteTurn)
                throw new InvalidOperationException("It is not this player's turn.");

            Board newBoard = Board.ApplyMove(move);

            Move completedMove = new Move(move.Id, move.From, move.To, move.PromotionPiece, FenConverterService.ToFen(newBoard));

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
                    .Select(m => GetRepetitionKey(m.FenAfterMove!))
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

        public static ChessGame Rehydrate(
            Guid id,
            string whitePlayerId,
            string? blackPlayerId,
            string initialFen,
            string currentFen,
            GameStatus status,
            GameEndReason? endReason,
            IEnumerable<Move> moveHistory)
        {
            var game = new ChessGame(
                id,
                whitePlayerId,
                currentFen);

            game.BlackPlayerId = blackPlayerId;
            game.InitialFen = initialFen;
            game.Status = status;
            game.EndReason = endReason;
            game.MoveHistory.AddRange(moveHistory);

            return game;
        }


    }
}
