using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.ViewGame
{
    public class ViewGameQueryHandler : IRequestHandler<ViewGameQuery, Result<ViewGameDto>>
    {
        private readonly IChessGameRepository _gameRepository;

        public ViewGameQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result<ViewGameDto>> Handle(ViewGameQuery query, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(query.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "Game not found.");

            bool isPlayer = game.WhitePlayerId == query.PlayerId || game.BlackPlayerId == query.PlayerId;

            if (!isPlayer)
                return Error.Unauthorized("Games.Unauthorized", "You are not a participant in this game.");

            ViewGameDto result = new ViewGameDto
            {
                GameId = game.Id,
                WhitePlayerId = game.WhitePlayerId,
                BlackPlayerId = game.BlackPlayerId,
                IsWhitePlayer = game.WhitePlayerId == query.PlayerId,
                Status = game.Status,
                CurrentTurn = game.Board.IsWhiteTurn
                ? "White"
                : "Black",
                Pieces = RetrievePieces(game)
            };

            return result;
        }

        private static IReadOnlyList<PieceDto> RetrievePieces(ChessGame game)
        {
            var pieces = new List<PieceDto>();

            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    var position = new Position(file, rank);
                    char piece = game.Board.GetPiece(position);

                    if (piece == '.')
                        continue;

                    pieces.Add(new PieceDto
                    {
                        File = file,
                        Rank = rank,
                        Piece = piece
                    });
                }
            }

            return pieces;
        }
    }
}
