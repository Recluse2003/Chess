using Chess.Application.Games.Commands.MakeMove;

namespace Chess.Web.Hubs.Clients
{
    /// <summary>
    /// Defines the client methods that can be invoked by the <see cref="ChessHub"/>. 
    /// </summary>
    public interface IChessClient
    {
        /// <summary>
        /// Notifies clients that a chess move has been made.
        /// </summary>
        /// <param name="result">
        /// The result of the completed move, including the board changes and updated game state.
        /// </param>
        Task MoveMade(MoveResultDto result);

        /// <summary>
        /// Notifies connected clients that a player has left the game.
        /// </summary>
        Task PlayerLeft();

        /// <summary>
        /// Notifies connected clients that the game has ended by mutual agreement.
        /// </summary>
        Task GameEndedByAgreement();
    }
}
