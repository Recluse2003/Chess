using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.VerifyGameMembershipQuery
{
    /// <summary>
    /// Handles the execution of a <see cref="VerifyGameMembershipQuery"/> request.
    /// Validates that the <see cref="ChessGame"/> exists, and that the user is actually a player of the game.
    /// </summary>
    public class VerifyGameMembershipQueryHandler : IRequestHandler<VerifyGameMembershipQuery, Result> 
    {
        private readonly IChessGameRepository _gameRepository;

        public VerifyGameMembershipQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="VerifyGameMembershipQuery"/>. Ensures the 
        /// <see cref="ChessGame"/> they are attempting to verify membership for exists, and that user is a 
        /// member of it.
        /// </summary>
        /// <param name="query">The details required to process the query.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful query will provide a <see cref="Result"/> stating so, which also represents that the 
        /// membership was verified. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> 
        /// stating the reason why.
        /// </returns>
        public async Task<Result> Handle(VerifyGameMembershipQuery query, CancellationToken cancellationToken)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(query.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            bool isPlayer = chessGame.WhitePlayerId == query.UserId || chessGame.BlackPlayerId == query.UserId;

            if (!isPlayer)
                return Error.Unauthorized("Games.NotPlayer", "You are not a player in this game.");

            return Result.Success();
        }
    }
}
