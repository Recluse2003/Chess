using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.ViewGame
{
    /// <summary>
    /// Handles the execution of a player's <see cref="ViewGameQuery"/> request. Validates the requester is allowed 
    /// to do so, and that the <see cref="ChessGame"/> they are trying to view the state of exists.
    /// </summary>
    public class ViewGameQueryHandler : IRequestHandler<ViewGameQuery, Result<ViewGameDto>>
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;

        public ViewGameQueryHandler(IChessGameRepository gameRepository, IUserRepository userRepository)
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="ViewGameQuery">. It also checks that the <see cref="ChessGame"> exists, and 
        /// the requester is allowed to view the game state.
        /// </summary>
        /// <param name="query">The details of the requested query.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful query will provide a <see cref="Result"/> containing the <see cref="ViewGameDto"/>. A failure 
        /// will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result<ViewGameDto>> Handle(ViewGameQuery query, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(query.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "Game not found.");

            bool isPlayer = game.WhitePlayerId == query.PlayerId || game.BlackPlayerId == query.PlayerId;

            if (!isPlayer)
                return Error.Unauthorized("Games.Unauthorized", "You are not a participant in this game.");

            string? whiteUsername = await _userRepository.GetUsernameByIdAsync(game.WhitePlayerId);
            string? blackUsername = await _userRepository.GetUsernameByIdAsync(game.BlackPlayerId!);

            ViewGameDto result = new ViewGameDto
            {
                GameId = game.Id,
                WhitePlayerUsername = whiteUsername!,
                BlackPlayerUsername = blackUsername!,
                IsWhitePlayer = game.WhitePlayerId == query.PlayerId,
                IsWhiteTurn = game.Board.IsWhiteTurn,
                Status = game.Status,
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
