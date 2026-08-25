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
        public bool IsWhiteTurn => Board.FullmoveNumber % 2 != 0;
        public List<Move> MoveHistory { get; private set; } = null!;

        public ChessGame(Guid id, string whitePlayerId, string blackPlayerId, string fen, GameStatus gameStatus) 
        { 
            this.Id = id;
            this.WhitePlayerId = whitePlayerId;
            this.BlackPlayerId = blackPlayerId;
            this.Status = gameStatus;

            Board = FenConverterService.FromFen(fen);
        }

        public void MakeMove(Move move, ChessRulesService rules)
        {
            // Checks if the game is even active
            if (Status != GameStatus.Active)
                throw new InvalidOperationException("Game is not in progress.");

            // Get the piece moving to check turn validity
            var piece = Board.GetPiece(move.From);
            if (char.IsUpper(piece) != IsWhiteTurn)
                throw new InvalidOperationException("It is not this player's turn.");

            // Verify move is legal
            if (!rules.IsMoveLegal(this, move))
                return;

            // Delegate the mutation to the board
            this.Board = Board.ApplyMove(move);

            // Records new move
            this.MoveHistory.Add(move);

            if (rules.IsCheckmate(Board)) 
                this.Status = GameStatus.Finished;
        }
    }
}
