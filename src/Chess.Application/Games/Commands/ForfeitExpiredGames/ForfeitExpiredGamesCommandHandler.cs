using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.ForfeitExpiredGames
{
    public class ForfeitExpiredGamesCommandHandler : IRequestHandler<ForfeitExpiredGamesCommand, Result<List<ExpiredGameDto>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public ForfeitExpiredGamesCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result<List<ExpiredGameDto>>> Handle(ForfeitExpiredGamesCommand request, CancellationToken cancellationToken)
        {
            List<ExpiredGameDto> expiredGames = await _gameRepository.ForfeitExpiredGamesAsync(cancellationToken);

            await _gameRepository.SaveChangesAsync();

            return expiredGames;
        }
    }
}
