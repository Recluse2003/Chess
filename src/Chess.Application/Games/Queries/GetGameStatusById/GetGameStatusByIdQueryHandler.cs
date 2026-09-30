using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameById
{
    public class GetGameStatusByIdQueryHandler : IRequestHandler<GetGameStatusByIdQuery, Result<GameStatus>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetGameStatusByIdQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result<GameStatus>> Handle(GetGameStatusByIdQuery request, CancellationToken cancellationToken)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(request.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            return chessGame.Status;
        }
    }
}
