using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameForReview
{
    public record GetGameForReviewQuery(Guid GameId, string UserId) : IRequest<Result<ChessGame>>;
}
