using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.RemoveAbandonedGames
{
    public record RemoveAbandonedGamesCommand : IRequest<Result>;
}
