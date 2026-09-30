using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Commands.ResumeGame
{
    public record ResumeGameCommand(Guid GameId, string UserId) : IRequest<Result>;
}
