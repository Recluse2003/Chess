using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Queries.GetRandomPublicGames
{
    public class GetRandomPublicGamesQueryHandler : IRequestHandler<GetRandomPublicGamesQuery, Result<List<PublicGameDto>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetRandomPublicGamesQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result<List<PublicGameDto>>> Handle(GetRandomPublicGamesQuery request, CancellationToken cancellationToken)
        {
            return await _gameRepository.GetRandomPublicGamesAsync(request.UserId, request.Count, cancellationToken);
        }
    }
}
