using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.PauseGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.ResumeGame
{
    /// <summary>
    /// Handles the execution of a player's <see cref="PauseGameCommand"/> request.
    /// Validates the game exists, and the requester is an actual player of the game, and that game is active.
    /// </summary>
    public class PauseGameCommandHandler : IRequestHandler<PauseGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public PauseGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="PauseGameCommand"/>. Validate the <see cref="ChessGame"/> exists, the 
        /// requester is a player of the game, and that the game is actually active.
        /// </summary>
        /// <param name="command">The details needed to pause the game.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful pause game request will provide a <see cref="Result"/> stating so. 
        /// A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result> Handle(PauseGameCommand command, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

            try
            {
                game.Pause(command.UserId);

                await _gameRepository.UpdateAsync(game);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (NotPlayerException ex)
            {
                return Error.Unauthorized("Games.NotPlayer", ex.Message);
            }
            catch (GameNotActiveException ex)
            {
                return Error.Conflict("Games.NotActive", ex.Message);
            }
        }
    }
}
