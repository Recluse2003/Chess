using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.EndGame;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Commands.PauseGame;
using Chess.Application.Games.Commands.ResumeGame;
using Chess.Application.Games.Queries.GetActiveGameByUserId;
using Chess.Application.Games.Queries.GetGameById;
using Chess.Application.Games.Queries.VerifyGameMembership;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;
using Chess.Web.Hubs.Clients;
using Chess.Web.Pages.Chess;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chess.Web.Hubs
{
    /// <summary>
    /// Provides real-time communication for active chess games.
    /// </summary>
    public class ChessHub : Hub<IChessClient>
    {
        private readonly IMediator _mediator; 

        public ChessHub(IMediator mediator) 
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Verifies that the connected user is a participant in the specified <see cref="ChessGame"/> 
        /// and adds their SignalR connection to the game's group. Resumes a chess game if the user is rejoining.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/> to join.</param>
        /// <exception cref="HubException">
        /// Thrown when the connected user is not authenticated or is not authorised to join the specified game.
        /// </exception>
        public async Task JoinGame(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new HubException("You must be logged in.");

            VerifyGameMembershipQuery verifyGameMembershipQuery = new(gameId, userId);

            Result verifyGameMembershipResult = await _mediator.Send(verifyGameMembershipQuery);

            if (!verifyGameMembershipResult.IsSuccess) 
                throw new HubException(verifyGameMembershipResult.Error!.Description);

            Result<ViewGameDto> viewGameResult = await _mediator.Send(new ViewGameQuery(gameId, userId));

            if (!viewGameResult.IsSuccess)
                throw new HubException(viewGameResult.Error!.Description);

            if (viewGameResult.Value.Status != GameStatus.Active && viewGameResult.Value.Status != GameStatus.Paused)
                throw new HubException("This match has already been completed or forfeited.");

            if (viewGameResult.Value.Status == GameStatus.Paused)
            {
                await _mediator.Send(new ResumeGameCommand(gameId, userId));

                await Groups.AddToGroupAsync(Context.ConnectionId, $"{gameId}");
                await Clients.Client(Context.ConnectionId).GameRejoined(viewGameResult.Value);

                await Clients.OthersInGroup($"{gameId}").OpponentReconnected();
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{gameId}");
            }
        }

        /// <summary>
        /// Attempts to make a chess move on behalf of the authenticated user.
        /// </summary>
        /// <param name="request">The move request containing the game and board positions involved.</param>
        /// <exception cref="HubException">
        /// Thrown when the connected user is not authenticated or the move cannot be completed.
        /// </exception>
        public async Task MakeMove(MakeMoveRequest request)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier); 
            
            if (userId == null) 
                throw new HubException("You must be logged in.");

            MakeMoveCommand command = new(
                request.GameId,
                userId,
                new Position(request.FromFile, request.FromRank),
                new Position(request.ToFile, request.ToRank),
                request.PromotionPiece);

            Result<MoveResultDto> result = await _mediator.Send(command);

            if (!result.IsSuccess)
                throw new HubException(result.Error!.Description);

            await Clients.Group($"{request.GameId}").MoveMade(result.Value);
        }

        /// <summary>
        /// Deals with a player disconnecting from an active chess game. 
        /// </summary>
        /// <param name="exception"></param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            Result<Guid> result = await _mediator.Send(new GetActiveGameByUserIdQuery(userId));

            if (result.IsSuccess)
            {
                Guid gameId = result.Value;

                await Clients.OthersInGroup($"{gameId}").OpponentDisconnected();

                await _mediator.Send(new PauseGameCommand(gameId, userId));
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
