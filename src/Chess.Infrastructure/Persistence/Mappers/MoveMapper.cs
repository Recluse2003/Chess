using Chess.Domain.ValueObjects;
using Chess.Infrastructure.Persistence.Models;

namespace Chess.Infrastructure.Persistence.Mappers
{
    /// <summary>
    /// Provides mapping operations between <see cref="MoveEntity"/> and <see cref="Move"/>.
    /// </summary>
    public static class MoveMapper
    {
        /// <summary>
        /// Converts a collection of <see cref="MoveEntity"/>s into a collection of <see cref="Move"/>s.
        /// </summary>
        /// <param name="moves">The collection of <see cref="MoveEntity"/>s to be converted.</param>
        /// <returns>A list of domain <see cref="Move"/> objects ordered by their move number.</returns>
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
