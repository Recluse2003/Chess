using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.ForfeitExpiredGames
{
    /// <summary>
    /// Handles the execution of the program's <see cref="ForfeitExpiredGamesCommand"/> request.
    /// </summary>
    public class ForfeitExpiredGamesCommandHandler : IRequestHandler<ForfeitExpiredGamesCommand, Result<List<ExpiredGameDto>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public ForfeitExpiredGamesCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="ForfeitExpiredGamesCommand"/> request. 
        /// </summary>
        /// <param name="request">The details needed to end expired games by forfeit.</param>
        /// <param name="cancellationToken">Triggers if the request is aborted early.</param>
        /// <returns></returns>
        public async Task<Result<List<ExpiredGameDto>>> Handle(ForfeitExpiredGamesCommand request, CancellationToken cancellationToken)
        {
            List<ExpiredGameDto> expiredGames = await _gameRepository.ForfeitExpiredGamesAsync(cancellationToken);

            await _gameRepository.SaveChangesAsync();

            return expiredGames;
        }
    }
}
