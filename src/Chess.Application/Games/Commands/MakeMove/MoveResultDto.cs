using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;


namespace Chess.Application.Games.Commands.MakeMove
{
    /// <summary>
    /// Represents the state of a <see cref="ChessGame"> after a successful move.
    /// </summary>
    /// <param name="Success">Indicates whether the move operation was successful.</param>
    /// <param name="BoardChanges">A list of <see cref="BoardChange"> resulting from the move.</param>
    /// <param name="Status">The current state of the chess game after the move.</param>
    /// <param name="EndReason">Provides the reason for the game ending after the move, if it was determined to be so.</param>
    public record MoveResultDto(
        bool Success,
        IReadOnlyList<BoardChange> BoardChanges,
        GameStatus Status,
        GameEndReason? EndReason = null
    );
}
