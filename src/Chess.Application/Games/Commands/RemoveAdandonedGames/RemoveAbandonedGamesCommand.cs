using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.RemoveAbandonedGames
{
    /// <summary>
    /// Contains the details required to remove abandoned games periodically.
    /// </summary>
    public record RemoveAbandonedGamesCommand() : IRequest<Result>;
}
