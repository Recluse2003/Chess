using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    /// <summary>
    /// Handles the execution of a player's <see cref="GetRandomPublicGamesQuery"/> request.
    /// </summary>
    public class GetRandomPublicGamesQueryHandler : IRequestHandler<GetRandomPublicGamesQuery, Result<List<PublicGameDto>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetRandomPublicGamesQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetRandomPublicGamesQuery"/>. Retrieves a random selection of available public 
        /// <see cref="ChessGame"/>s to join from the chess game repository. 
        /// </summary>
        /// <param name="request">The details required to retrieve the random public games.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation early, if required.</param>
        /// <returns>
        /// A <see cref="Result"/> containing a list of randomly selected public games available to join.
        /// </returns>
        public async Task<Result<List<PublicGameDto>>> Handle(GetRandomPublicGamesQuery request, CancellationToken cancellationToken)
        {
            return await _gameRepository.GetRandomPublicGamesAsync(request.UserId, request.Count, cancellationToken);
        }
    }
}
