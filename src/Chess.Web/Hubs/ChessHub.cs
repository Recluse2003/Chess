using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.VerifyGameMembershipQuery;
using Chess.Domain.ValueObjects;
using Chess.Web.Hubs.Clients;
using Chess.Web.Pages.Chess;
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

            VerifyGameMembershipQuery query = new(gameId, userId);

            Result result = await _mediator.Send(query);

            if (!result.IsSuccess) 
                throw new HubException(result.Error!.Description);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
        }

        public async Task MakeMove(MakeMoveRequest request)
        {
            MakeMoveCommand command = new(
                request.GameId,
                "whitePlayer",
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
