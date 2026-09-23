using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.StartGame;
using Chess.Application.Games.Queries.GetGameLobby;
using Chess.Domain.Entities;
using Chess.Web.Hubs.Clients;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chess.Web.Hubs
{
    /// <summary>
    /// Provides real-time communication for chess game lobbies. 
    /// </summary>
    public class LobbyHub : Hub<ILobbyClient>
    {
        private readonly IMediator _mediator;

        public LobbyHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Verifies that the authenticated user is a participant in the specified <see cref="ChessGame"/>
        /// and adds their SignalR connection to the game's lobby group.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/>.</param>
        /// <exception cref="HubException">
        /// Thrown when the connected user is not authenticated or is not authorised to join the specified 
        /// lobby.
        /// </exception>
        public async Task JoinLobby(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new HubException("You must be logged in.");

            GetGameLobbyQuery query = new(gameId, userId);

            Result<GameLobbyDto> result = await _mediator.Send(query);

            if (!result.IsSuccess)
                throw new HubException(result.Error!.Description);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");

            if (!result.Value.IsWhitePlayer)
                await Clients.OthersInGroup($"game-{gameId}").PlayerJoined(result.Value.BlackPlayerUsername);
        }

        /// <summary>
        /// Starts the specified <see cref="ChessGame"/> for the authenticated player. Only the host may start
        /// the game.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/> to start.</param>
        /// <exception cref="HubException">
        /// Thrown when the connected user is not authenticated or the game cannot be started.
        /// </exception>
        public async Task StartGame(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new HubException("You must be logged in.");

            StartGameCommand command = new(gameId, userId);

            Result result = await _mediator.Send(command);

            if (!result.IsSuccess)
                throw new HubException(result.Error!.Description);

            await Clients.Group($"game-{gameId}").GameStarted();
        }
    }
}