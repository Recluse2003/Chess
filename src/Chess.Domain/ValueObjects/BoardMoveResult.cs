namespace Chess.Domain.ValueObjects
{
    public record BoardMoveResult(Board Board, IReadOnlyList<BoardChange> BoardChanges);
}
