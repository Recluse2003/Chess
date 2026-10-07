using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameById
{
    /// <summary>
    /// Handles the execution of a player's <see cref="GetGameStatusByIdQuery"/> request.
    /// Validates the game exists.
    /// </summary>
    public class GetGameStatusByIdQueryHandler : IRequestHandler<GetGameStatusByIdQuery, Result<GameStatus>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetGameStatusByIdQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetGameStatusByIdQuery"/>. Validates the <see cref="ChessGame"/> exists.
        /// </summary>
        /// <param name="command">The details needed to pause the game.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful get game status request will provide a <see cref="Result"/> stating so, along with the current game 
        /// status of the specified <see cref="ChessGame"/>. A failure will provide a <see cref="Result"/> containing an 
        /// <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result<GameStatus>> Handle(GetGameStatusByIdQuery command, CancellationToken cancellationToken)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            return chessGame.Status;
        }
    }
}
