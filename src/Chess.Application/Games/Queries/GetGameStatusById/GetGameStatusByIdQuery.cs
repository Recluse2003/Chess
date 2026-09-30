using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameById
{
    public record GetGameStatusByIdQuery(Guid GameId) : IRequest<Result<GameStatus>>;
}
