using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.ForfeitExpiredGames
{
    public record ForfeitExpiredGamesCommand() : IRequest<Result<List<ExpiredGameDto>>>;
}
