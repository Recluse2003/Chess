using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.GetLegalMoves
{
    /// <summary>
    /// Contains the details required to retrieve of the legal moves of a piece in a specified <see cref="ChessGame"/>
    /// </summary>
    /// <param name="GameId">Identifier of the <see cref="ChessGame"/>.</param>
    /// <param name="PlayerId">Identifier of the player attempting to retrieve the legal moves.</param>
    /// <param name="PiecePosition">Position of the piece the player is attempting to retrieve the legal moves for.</param>
    public record GetLegalMovesQuery(Guid GameId, string PlayerId, Position PiecePosition) : IRequest<Result<List<LegalMoveDto>>>;
}
