using Chess.Domain.Enums;
using Chess.Domain.Exceptions;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Entities
{
    /// <summary>
    /// Represents the central Domain Aggregate Root for a chess match.
    /// Enforces business invariants, manages lifecycle states, and processes piece movements.
    /// </summary>
    public class ChessGame
    {
        public Guid Id { get; private set; }
        public string WhitePlayerId { get; private set; }
        public string? BlackPlayerId { get; private set; } 
        public string InitialFen { get; private set; }
        public Board Board { get; private set; } = null!;
        public GameStatus Status { get; private set; } = GameStatus.Active;
        public GameEndReason? EndReason { get; private set; }
        public List<Move> MoveHistory { get; private set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChessGame"/> aggregate in a waiting state.
        /// </summary>
        /// <param name="id">The unique identifier to assign to this game match.</param>
        /// <param name="whitePlayerId">The identifier of the player creating and hosting the match.</param>
        /// <param name="fen">The starting position setup format string.</param>
        public ChessGame(Guid id, string whitePlayerId, string fen) 
        { 
            Id = id;
            WhitePlayerId = whitePlayerId;

            InitialFen = fen;
            Board = FenConverterService.FromFen(fen);

            Status = GameStatus.WaitingForOpponent;
        }

        /// <summary>
        /// Enables a player to join a <see cref="ChessGame"/> in a <see cref="GameStatus.WaitingForOpponent"/> state.
        /// </summary>
        /// <param name="playerId">The identifier of the player attempting to fill the Black pieces slot.</param>
        /// <exception cref="GameStateTransitionException">
        /// Thrown if the game lobby is not in a receptive state, or if the spot is already occupied.
        /// </exception>
        public void Join(string playerId)
        {
            if (Status != GameStatus.WaitingForOpponent)
                throw GameStateTransitionException.NotAcceptingPlayers();

            if (BlackPlayerId != null)
                throw GameStateTransitionException.OpponentAlreadyJoined();

            if (WhitePlayerId == playerId)
               throw new GameStateTransitionException("You cannot join your own game.");

            BlackPlayerId = playerId;
            Status = GameStatus.NotStarted;
        }

        /// <summary>
        /// Changes the state of a <see cref="ChessGame"/> to <see cref="GameStatus.Active"/>, representing the 
        /// start and play of the game.
        /// </summary>
        /// <param name="playerId">The identifier of the host attempting to initiate the game.</param>
        /// <exception cref="GameStateTransitionException">
        /// Thrown if the match is already running, lacks an opponent, or if the caller is not the authenticated match host.
        /// </exception>
        public void Start(string playerId)
        {
            if (Status != GameStatus.NotStarted)
                throw GameStateTransitionException.MatchAlreadyStarted();

            if (BlackPlayerId == null)
                throw GameStateTransitionException.OpponentMissing();

            if (WhitePlayerId != playerId)
                throw GameStateTransitionException.NotTheHost();

            Status = GameStatus.Active;
        }

        /// <summary>
        /// Performs a change to the chess game's board using the specified move.
        /// </summary>
        /// <param name="move">The move to be executed in the chess game.</param>
        /// <returns>A read-only list of all changes done to the chess game's board.</returns>
        /// <exception cref="GameNotActiveException">Thrown if an execution is attempted on a non-active match.</exception>
        /// <exception cref="InvalidTurnException">Thrown if the piece color alignment violates the current sequence clock.</exception>
        public IReadOnlyList<BoardChange> MakeMove(Move move)
        {
            if (Status != GameStatus.Active)
                throw new GameNotActiveException();

            // Get the piece moving to check turn validity
            char piece = Board.GetPiece(move.From);
            if (char.IsUpper(piece) != Board.IsWhiteTurn)
                throw new InvalidTurnException();

            BoardMoveResult moveResult = Board.ApplyMove(move);

            Move completedMove = new Move(move.Id, move.From, move.To, move.PromotionPiece, FenConverterService.ToFen(moveResult.Board));

            Board = moveResult.Board;
            MoveHistory.Add(completedMove);

            return moveResult.BoardChanges;
        }

        /// <summary>
        /// Evaluates the current state of the game to determine if the game has reached one of the possible endgame 
        /// states, and updates the game state to reflect this.
        /// </summary>
        /// <param name="rules">
        /// The service containing the chess game logic, to be used to determine if the game has
        /// reached a possible endgame state.
        /// </param>
        public void CheckGameState(ChessRulesService rules)
        {
            if (Status != GameStatus.Active)
                return;

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

        /// <summary>
        /// Checks if the specified playerid is able to move a piece in the game, in its current turn.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public bool CanPlayerMove(string playerId)
        {
            if (Status != GameStatus.Active)
                return false;

            bool isWhitePlayer = playerId == WhitePlayerId;
            bool isBlackPlayer = playerId == BlackPlayerId;

            if (!isWhitePlayer && !isBlackPlayer)
                return false;

            return isWhitePlayer == Board.IsWhiteTurn;
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
