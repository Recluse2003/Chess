namespace Chess.Domain.ValueObjects
{
    /// <summary>
    /// Contains the new <see cref="ValueObjects.Board"/>, and the <see cref="BoardChange"/>s that are returned after a 
    /// move is made. 
    /// </summary>
    /// <param name="Board">The new <see cref="ValueObjects.Board"/>.</param>
    /// <param name="BoardChanges">A readonly list of all the differences between the old and new
    /// <see cref="ValueObjects.Board"/>s.</param>
    public record BoardMoveResult(Board Board, IReadOnlyList<BoardChange> BoardChanges);
}
