using Chess.Domain.ValueObjects;
using Chess.Infrastructure.Persistence.Models;

namespace Chess.Infrastructure.Persistence.Mappers
{
    public static class MoveMapper
    {
        public static List<Move> ToDomainList(List<MoveEntity> moves)
        {
            return moves
                .OrderBy(move => move.MoveNumber)
                .Select(move => new Move(move.Id,
                    Position.FromChessNotation(move.From),
                    Position.FromChessNotation(move.To),
                    move.PromotionPiece,
                    move.FenAfterMove))
                .ToList(); 
        }
    }
}
