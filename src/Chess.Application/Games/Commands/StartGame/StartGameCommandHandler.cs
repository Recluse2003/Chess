using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.StartGame
{
    /// <summary>
    /// Handles the execution of a game start request. 
    /// Validates the user requesting it is allowed to do so, and that the specified game exists.
    /// </summary>
    public class StartGameCommandHandler : IRequestHandler<StartGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly IGameCodeRepository _codeRepository;

        public StartGameCommandHandler(IChessGameRepository gameRepository, IGameCodeRepository codeRepository)
        {
            _gameRepository = gameRepository;
            _codeRepository = codeRepository;
        }

        /// <summary>
        /// Processes the incoming start game command. Ensures the game exists, and that the requester is allowed to
        /// do so.
        /// </summary>
        /// <param name="command">The details of the requested game start command.</param>
        /// <param name="token">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful start will provide a <see cref="Result"/> stating so, while a failure will result in an <see cref="Error"/>.
        /// </returns>
        public async Task<Result> Handle(StartGameCommand command, CancellationToken token)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

            try
            {
                chessGame.Start(command.PlayerId);

                await _gameRepository.UpdateAsync(chessGame);
                await _codeRepository.DeleteCodeByGameIdAsync(chessGame.Id);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (GameStateTransitionException ex)
            {
                return Error.Conflict("Games.StartFailed", ex.Message);
            }
        }
    }
}
