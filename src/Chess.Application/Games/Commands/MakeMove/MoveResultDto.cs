using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;


namespace Chess.Application.Games.Commands.MakeMove
{
    public record MoveResultDto(
        bool Success,
        IReadOnlyList<BoardChange> BoardChanges,
        GameStatus Status,
        GameEndReason? EndReason = null
    );
}
