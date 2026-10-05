using Chess.Application.Interfaces;
using Chess.Web.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Chess.Web.Hubs
{
    public class SignalRGameNotifier : IGameNotifier
    {
        private readonly IHubContext<ChessHub, IChessClient> _hubContext;

        public SignalRGameNotifier(IHubContext<ChessHub, IChessClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task GameEndedByForfeit(Guid gameId, bool isWhiteWinner)
        {
            await _hubContext.Clients.Group(gameId.ToString()).GameEndedByForfeit(isWhiteWinner);
        }
    }
}
