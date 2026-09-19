using Chess.Application.Common.Results;
using MediatR;


namespace Chess.Application.Games.Queries.ViewGame
{
    public record ViewGameQuery(Guid GameId, string PlayerId) : IRequest<Result<ViewGameDto>>;
}
