using Microsoft.AspNetCore.SignalR;

namespace Chess.Web.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinGame(Guid gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
        }

        public async Task StartGame(Guid gameId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("GameStarted");
        }
    }
}
