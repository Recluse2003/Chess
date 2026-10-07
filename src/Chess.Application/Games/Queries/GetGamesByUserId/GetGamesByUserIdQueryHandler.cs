using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Queries.GetGamesByUserId
{
    /// <summary>
    /// Handles the execution of a player's <see cref="GetGamesByUserIdQuery"/> request.
    /// </summary>
    public class GetGamesByUserIdQueryHandler : IRequestHandler<GetGamesByUserIdQuery, Result<PagedList<GameDto>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetGamesByUserIdQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetGamesByUserIdQuery"/> request, by retrieving the user's games from the 
        /// chess game repository
        /// </summary>
        /// <param name="request">Contains the details needed to retrieve the specified player's games.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A successful get games query will provide a <see cref="Result"/> stating so, which contains a paginated list of
        /// the user's games.
        /// </returns>
        public async Task<Result<PagedList<GameDto>>> Handle(GetGamesByUserIdQuery request, CancellationToken cancellationToken)
        {
            return await _gameRepository.GetGamesByUserIdAsync(request.UserId, request.PageNumber, request.PageSize);
        }
    }
}
