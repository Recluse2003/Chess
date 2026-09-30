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
        /// and adds their SignalR connection to the game's group.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/> to join.</param>
        /// <exception cref="HubException">
        /// Thrown when the connected user is not authenticated or is not authorised to join the specified game.
        /// </exception>
        public async Task JoinGame(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new HubException();

            VerifyGameMembershipQuery query = new(gameId, userId);

            Result result = await _mediator.Send(query);

            if (!result.IsSuccess) 
                throw new HubException(result.Error!.Description);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
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

            await Clients.Group($"game-{request.GameId}").MoveMade(result.Value);
        }

        /// <summary>
        /// Attempts to rejoin a <see cref="ChessGame"/> a player has disconnected from. 
        /// </summary>
        /// <param name="gameId">The id of the <see cref="ChessGame"/> the player is attempting to rejoin.</param>
        /// <exception cref="HubException">
        /// Thrown when the connected user is not authenticated or the player was unable to verified to a part of the game
        /// they attempted to rejoin, or if attempting to retrieve the current game state failed. 
        /// </exception>
        public async Task RejoinGame(Guid gameId)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) 
                throw new HubException("You must be logged in.");

            Result<ViewGameDto> result = await _mediator.Send(new ViewGameQuery(gameId, userId));

            if (!result.IsSuccess)
                throw new HubException(result.Error!.Description);

            if (result.Value.Status != GameStatus.Paused)
                throw new HubException("This match has already been completed or forfeited.");

            await _mediator.Send(new ResumeGameCommand(gameId, userId));

            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
            await Clients.Client(Context.ConnectionId).GameRejoined(result.Value);

            await Clients.OthersInGroup($"game-{gameId}").OpponentReconnected();
        }

        /// <summary>
        /// Deals with a player disconnecting from an active chess game. 
        /// </summary>
        /// <param name="exception"></param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string? userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            { 
                await base.OnDisconnectedAsync(exception); 
                return; 
            }

            GetActiveGameByUserIdQuery activeGameQuery = new(userId);
            Result<Guid> gameIdResult = await _mediator.Send(activeGameQuery);

            if (gameIdResult.IsSuccess)
            {
                Guid gameId = gameIdResult.Value;

                // Tell the opponent to show the "Opponent Disconnected" banner
                await Clients.OthersInGroup($"game-{gameId}").OpponentDisconnected();

                // Fire a command to set the DB status to 'Paused'
                await _mediator.Send(new PauseGameCommand(gameId, userId));

                // Start the 60-second timer in the background.
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(60));

                    // Fetch the game's state from the database
                    GetGameStatusByIdQuery checkGameStatusQuery = new (gameId);
                    Result<GameStatus> gameStatusResult = await _mediator.Send(checkGameStatusQuery);

                    // If the player never rejoined, the game would still be in a paused state. 
                    if (gameStatusResult.IsSuccess && gameStatusResult.Value == GameStatus.Paused)
                    {
                        // Forfeit the player who disconnected
                        EndGameCommand command = new (gameId, userId, GameEndReason.Disconnect);
                        await _mediator.Send(command);

                        var finalStatusQuery = new GetGameStatusByIdQuery(gameId);
                        var finalStatusResult = await _mediator.Send(finalStatusQuery);

                        if (finalStatusResult.IsSuccess)
                        {
                            bool isWhiteWinner = finalStatusResult.Value == GameStatus.WhiteWin;
                            await Clients.Group($"game-{gameId}").GameEndedByForfeit(isWhiteWinner);
                        }
                    }
                });
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
