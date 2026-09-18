using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;

namespace Chess.Application.Games.Queries.GetLegalMoves
{
    public class GetLegalMovesQueryHandler
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly ChessRulesService _rules;

        public GetLegalMovesQueryHandler(IChessGameRepository gameRepository, ChessRulesService rules)
        {
            _gameRepository = gameRepository;
            _rules = rules;
        }

        public async Task<List<LegalMoveDto>> ExecuteAsync(GetLegalMovesQuery query)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(query.GameId);

            if (game == null)
                throw new InvalidOperationException("Game not found.");

            // if (!game.CanPlayerMove(query.PlayerId))
               // return [];

            char piece = game.Board.GetPiece(query.PiecePosition);

            if (piece == '.' || game.Board.IsWhiteTurn != char.IsUpper(piece))
                return [];

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
