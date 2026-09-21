using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.GetGameCode;
using Chess.Web.Hubs.Clients;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chess.Web.Hubs
{
    public class ChessHub : Hub<IChessClient>
    {
        private readonly IMediator _mediator; 

        public ChessHub(IMediator mediator) 
        {
            _mediator = mediator;
        }

        public async Task JoinGame(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                throw new HubException();

            GetGameLobbyQuery query = new(gameId, userId);

            Result<GameLobbyDto> result = await _mediator.Send(query);

            if (!result.IsSuccess) 
                throw new HubException();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");

            string? username = result.Value.IsWhitePlayer ? result.Value.WhitePlayerUsername : result.Value.BlackPlayerUsername;

            await Clients.Group($"game-{gameId}").PlayerJoined(result.Value.IsWhitePlayer, username!);
        }

        public async Task StartGame(Guid gameId)
        {
            await Clients.Group($"game-{gameId}").GameStarted();
        }
    }
}
