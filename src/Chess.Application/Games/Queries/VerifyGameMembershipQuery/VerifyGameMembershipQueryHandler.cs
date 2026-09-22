using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.VerifyGameMembershipQuery
{
    public class VerifyGameMembershipQueryHandler : IRequestHandler<VerifyGameMembershipQuery, Result> 
    {
        private readonly IChessGameRepository _gameRepository;

        public VerifyGameMembershipQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

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
