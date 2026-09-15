using Chess.Domain.ValueObjects;

namespace Chess.Application.Games.Queries.GetLegalMoves
{
    public record GetLegalMovesQuery(Guid GameId, string CurrentUserId, Position PiecePosition);
}
