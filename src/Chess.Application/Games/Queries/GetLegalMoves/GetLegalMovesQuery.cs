using Chess.Application.Common.Results;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.GetLegalMoves
{
    public record GetLegalMovesQuery(Guid GameId, string PlayerId, Position PiecePosition) : IRequest<Result<List<LegalMoveDto>>>;
}
