using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.ResumeGame
{
    /// <summary>
    /// Handles the execution of a user's <see cref="ResumeGameCommand"/> request.
    /// Validates the <see cref="ChessGame"/> exists, that the user is player of the game, and that game is actually paused.
    /// </summary>
    public class ResumeGameCommandHandler : IRequestHandler<ResumeGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public ResumeGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="ResumeGameCommand"/>. Validate the <see cref="ChessGame"/> exists, the 
        /// requester is a player of the game, and that the game is actually paused.
        /// </summary>
        /// <param name="command">The details needed to resume the game.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful resume game request will provide a <see cref="Result"/> stating so. 
        /// A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result> Handle(ResumeGameCommand command, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

            try
            {
                game.Resume(command.UserId);

                await _gameRepository.UpdateAsync(game);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (NotPlayerException ex)
            {
                return Error.Unauthorized("Games.NotPlayer", ex.Message);
            }
            catch (GameNotPausedException ex)
            {
                return Error.Conflict("Games.NotPaused", ex.Message);
            }
        }
    }
}
