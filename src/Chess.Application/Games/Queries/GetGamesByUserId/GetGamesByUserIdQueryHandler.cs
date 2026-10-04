using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using MediatR;

namespace Chess.Application.Games.Queries.GetGamesByUserId
{
    public class GetGamesByUserIdQueryHandler : IRequestHandler<GetGamesByUserIdQuery, Result<PagedList<GameDto>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetGamesByUserIdQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result<PagedList<GameDto>>> Handle(GetGamesByUserIdQuery request, CancellationToken cancellationToken)
        {
            return await _gameRepository.GetGamesByUserIdAsync(request.UserId, request.PageNumber, request.PageSize);
        }
    }
}
