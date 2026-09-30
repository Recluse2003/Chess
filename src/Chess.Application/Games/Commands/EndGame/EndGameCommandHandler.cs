using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.EndGame
{
    public class EndGameCommandHandler : IRequestHandler<EndGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public EndGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result> Handle(EndGameCommand request, CancellationToken cancellationToken)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(request.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            bool isPlayer = chessGame.WhitePlayerId == request.UserId ||
                            chessGame.BlackPlayerId == request.UserId;

            if (!isPlayer)
                return Error.Unauthorized("Games.NotPlayer", "You are not a player in this game.");

            chessGame.EndGame(request.Reason, request.UserId);

            await _gameRepository.UpdateAsync(chessGame);
            await _gameRepository.SaveChangesAsync();

            return Result.Success();
        }
    }
}
