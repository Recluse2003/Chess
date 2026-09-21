using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.GetLegalMoves
{
    /// <summary>
    /// Handles the execution of a player's <see cref="GetLegalMovesQuery"/> request. Validates that the <see cref="ChessGame"/>
    /// exists, and that it's the turn of the player requesting and that they exist.
    /// </summary>
    public class GetLegalMovesQueryHandler : IRequestHandler<GetLegalMovesQuery, Result<List<LegalMoveDto>>>
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly ChessRulesService _rules;

        public GetLegalMovesQueryHandler(IChessGameRepository gameRepository, ChessRulesService rules)
        {
            _gameRepository = gameRepository;
            _rules = rules;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetLegalMovesQuery"/>. Ensures the <see cref="ChessGame"/> that player is 
        /// attempting to get moves for exists, and that its the player's turn and that they exist.  
        /// </summary>
        /// <param name="query">The details required to retrieve the moves.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful get legal moves request will provide a <see cref="Result"/> containing a list of all the legal 
        /// moves of a specified piece. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/>
        /// stating the reason why.
        /// </returns>
        public async Task<Result<List<LegalMoveDto>>> Handle(GetLegalMovesQuery query, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(query.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

            if (!game.CanPlayerMove(query.PlayerId))
                return Error.Conflict("Games.NotYourTurn", "It is currently not your turn to move.");

            char piece = game.Board.GetPiece(query.PiecePosition);

            if (piece == '.' || game.Board.IsWhiteTurn != char.IsUpper(piece))
                return new List<LegalMoveDto>();

            List<Position> legalMoves = _rules.GetLegalMoves(game.Board, query.PiecePosition);

            List<LegalMoveDto> result = legalMoves
                .Select(move => new LegalMoveDto
                {
                    File = move.File,
                    Rank = move.Rank
                })
                .ToList();

            return result;
        }
    }
}
