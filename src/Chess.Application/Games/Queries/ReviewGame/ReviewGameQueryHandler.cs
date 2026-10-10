using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.ReviewGame
{
    public class ReviewGameQueryHandler : IRequestHandler<ReviewGameQuery, Result<ReviewGameDto>>
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;

        public ReviewGameQueryHandler(IChessGameRepository gameRepository, IUserRepository userRepository)
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<ReviewGameDto>> Handle(ReviewGameQuery request, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(request.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "Game not found.");

            bool isPlayer = game.WhitePlayerId == request.UserId || game.BlackPlayerId == request.UserId;

            if (!isPlayer)
                return Error.Unauthorized("Games.Unauthorized", "You are not a participant in this game.");

            string? whiteUsername = await _userRepository.GetUsernameByIdAsync(game.WhitePlayerId);
            string? blackUsername = await _userRepository.GetUsernameByIdAsync(game.BlackPlayerId!);

            ReviewGameDto result = new()
            {
                GameId = game.Id,
                WhitePlayerUsername = whiteUsername!,
                BlackPlayerUsername = blackUsername!,
                IsWhitePlayer = game.WhitePlayerId == request.UserId,
                IsWhiteTurn = game.Board.IsWhiteTurn,

                Status = game.Status,
                EndReason = game.EndReason,

                NumberOfMoves = game.MoveHistory.Count,
                Pieces = RetrievePieces(game)
            };

            return result;
        }

        /// <summary>
        /// Retrieves all the pieces within a specified <see cref="ChessGame"/>, the positions of said pieces.
        /// </summary>
        /// <param name="game">The <see cref="ChessGame"/> the pieces are being retrieved from.</param>
        /// <returns>A list of <see cref="PieceDto"/>, which states the position of each piece, and what type of piece.</returns>
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
