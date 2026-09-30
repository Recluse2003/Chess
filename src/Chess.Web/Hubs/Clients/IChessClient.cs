using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.ViewGame;

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
        /// Notifies connected clients that the opponent has disconnected, and is attempting 
        /// reconnect.
        /// </summary>
        Task OpponentDisconnected();

        /// <summary>
        /// Notifies connected clients that the disconnected opponent reconnected.
        /// </summary>
        Task OpponentReconnected();

        /// <summary>
        /// Notifies connected clients that the disconnected opponent reconnected.
        /// </summary>
        /// <param name="chessGame">
        /// The current state of the game after the reconnect. 
        /// </param>
        Task GameRejoined(ViewGameDto chessGame);

        /// <summary>
        /// Notifies connected clients that the game ended by forfeit.
        /// </summary>
        /// <param name="isWhiteWin">
        /// If true, white wins by default, as black player forfeited
        /// </param>
        Task GameEndedByForfeit(bool isWhiteWin);

        /// <summary>
        /// Notifies connected clients that the game has ended by mutual agreement.
        /// </summary>
        Task GameEndedByAgreement();
    }
}
