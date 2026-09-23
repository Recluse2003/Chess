namespace Chess.Web.Hubs.Clients
{
    /// <summary>
    /// Defines the client methods that can be invoked by the <see cref="LobbyHub"/>. 
    /// </summary>
    public interface ILobbyClient 
    {
        /// <summary>
        /// Notifies connected clients that a black player has joined the lobby.
        /// </summary>
        /// <param name="blackPlayerUsername">The username of the black player who joined the lobby.</param>
        Task PlayerJoined(string? blackPlayerUsername);

        /// <summary>
        /// Notifies connected clients that a player has left the lobby.
        /// </summary>
        Task PlayerLeft();

        /// <summary>
        /// Notifies connected clients that the game has started.
        /// </summary>
        Task GameStarted();
    }
}
