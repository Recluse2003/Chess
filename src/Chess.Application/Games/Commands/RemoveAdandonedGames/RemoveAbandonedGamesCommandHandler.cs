using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.RemoveAbandonedGames
{
    /// <summary>
    /// Handles the execution of a <see cref="RemoveAbandonedGamesCommand"/> request.
    /// </summary>
    public class RemoveAbandonedGamesCommandHandler : IRequestHandler<RemoveAbandonedGamesCommand, Result> 
    {
        private readonly IChessGameRepository _gameRepository;

        public RemoveAbandonedGamesCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="RemoveAbandonedGamesCommand"/>.
        /// </summary>
        /// <param name="command">The details needed to remove abadoned games.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful remove abandoned games request will provide a <see cref="Result"/> stating so. 
        /// A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result> Handle(RemoveAbandonedGamesCommand command, CancellationToken cancellationToken)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-1);

            await _gameRepository.DeleteUnstartedGamesOlderThanAsync(cutoffTime, cancellationToken);

            await _gameRepository.SaveChangesAsync();

            return Result.Success();
        }
    }
}
