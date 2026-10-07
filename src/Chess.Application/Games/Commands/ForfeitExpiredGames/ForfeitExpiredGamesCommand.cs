using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.ForfeitExpiredGames
{
    /// <summary>
    /// Contains the details required to end a game without being done through a move.
    /// </summary>
    public record ForfeitExpiredGamesCommand() : IRequest<Result<List<ExpiredGameDto>>>;
}
