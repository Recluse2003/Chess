using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.VerifyGameMembershipQuery;
using Chess.Domain.Entities;
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

            if (userId is null)
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
            
            if (userId is null) 
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
    }
}
