using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Commands.EndGame
{
    /// <summary>
    /// Handles the execution of a player's <see cref="EndGameCommand"/> request.
    /// Validates game exists, and the requester is an actual player of the game.
    /// </summary>
    public class EndGameCommandHandler : IRequestHandler<EndGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public EndGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="EndGameCommand"/> request. Ensures the <see cref="ChessGame"/> exists, 
        /// and that the requester is a player of the game.
        /// </summary>
        /// <param name="request">The details needed to end the game.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful end game request will provide a <see cref="Result"/> stating so. 
        /// A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
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
