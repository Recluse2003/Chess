using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.RemoveAbandonedGames
{
    public class RemoveAbandonedGamesCommandHandler : IRequestHandler<RemoveAbandonedGamesCommand, Result> 
    {
        private readonly IChessGameRepository _gameRepository;

        public RemoveAbandonedGamesCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result> Handle(RemoveAbandonedGamesCommand command, CancellationToken cancellationToken)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-1);

            await _gameRepository.DeleteUnstartedGamesOlderThanAsync(cutoffTime, cancellationToken);

            await _gameRepository.SaveChangesAsync();

            return Result.Success();
        }
    }
}
