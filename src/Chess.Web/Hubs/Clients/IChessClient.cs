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
        /// Notifies connected clients that the game ended due to timeout.
        /// </summary>
        /// <param name="isWhiteWinner">
        /// If true, white wins by default, as black player timedout. 
        /// </param>
        Task GameEndedByTimeout(bool isWhiteWinner);

        /// <summary>
        /// Notifies connected clients that the game ended due to forfeit.
        /// </summary>
        /// <param name="isWhiteWinner">
        /// If true, white wins as black forfeited, else black won because white forfeited. 
        /// </param>
        Task GameEndedByForfeit(bool isWhiteWinner);

        /// <summary>
        /// Notifies connected clients that a draw offer was sent, and is waiting for a response. 
        /// </summary>
        Task DrawOffered();

        /// <summary>
        /// Notifies connected clients that the draw offer was declined.
        /// </summary>
        Task DrawOfferDeclined();

        /// <summary>
        /// Notifies connected clients that the draw offer was accepted, and the game has ended by mutual agreement.
        /// </summary>
        Task GameEndedByMutualAgreement();
    }
}
