namespace Chess.Web.Hubs.Clients
{
    public interface ILobbyClient 
    {
        Task PlayerJoined(string? blackPlayerUsername);
        Task PlayerLeft();
        Task GameStarted();
    }
}
