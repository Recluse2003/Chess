using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Commands.MakeMove
{
    /// <summary>
    /// Contains the required data to perform a chess move. 
    /// </summary>
    /// <param name="GameId">The identifier of the <see cref="ChessGame">.</param>
    /// <param name="PlayerId">The identifier of the player attempting to make a move.</param>
    /// <param name="From">The coordinate <see cref="Position"> of the piece being moved.</param>
    /// <param name="To">The coordinate <see cref="Position"> the piece is being moved to.</param>
    /// <param name="PromotionPiece">
    /// The <see cref="char"> representation of the chosen piece if a pawn promotes (e.g., 'q', 'r', 'b', 'n'). 
    /// Null otherwise.
    /// </param>
    public record MakeMoveCommand(Guid GameId, string PlayerId, Position From, Position To, char? PromotionPiece) : IRequest<Result<MoveResultDto>>;
}
