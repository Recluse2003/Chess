using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.StartGame;
using Chess.Application.Games.Queries.GetGameCode;
using Chess.Web.Hubs.Clients;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Chess.Web.Hubs
{
    public class LobbyHub : Hub<ILobbyClient>
    {
        private readonly IMediator _mediator;

        public LobbyHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task JoinLobby(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                throw new HubException();

            GetGameLobbyQuery query = new(gameId, userId);

            Result<GameLobbyDto> result = await _mediator.Send(query);

            if (!result.IsSuccess)
                throw new HubException();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");

            if (!result.Value.IsWhitePlayer)
                await Clients.OthersInGroup($"game-{gameId}").PlayerJoined(result.Value.BlackPlayerUsername);
        }

        public async Task StartGame(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                throw new HubException("You must be logged in.");

            StartGameCommand command = new(gameId, userId);

            Result result = await _mediator.Send(command);

            if (!result.IsSuccess)
                throw new HubException(result.Error!.Description);

            await Clients.Group($"game-{gameId}").GameStarted();
        }
    }
}