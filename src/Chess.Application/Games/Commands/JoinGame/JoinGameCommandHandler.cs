using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.JoinGame
{
    /// <summary>
    /// Handles the execution of a player's <see cref="JoinGameCommand"/> request.
    /// Validates that the game exists, that they are able to, and updates the game in the database.
    /// </summary>
    public class JoinGameCommandHandler : IRequestHandler<JoinGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public JoinGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="JoinGameCommand"/>. Ensures the <see cref="ChessGame"/> that player is 
        /// attempting to join exists, and that the player also exists.
        /// </summary>
        /// <param name="command">The details required to perform the join request.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful join game request will provide a <see cref="Result"/> stating so. 
        /// A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result> Handle(JoinGameCommand command, CancellationToken cancellationToken)
        {
            try
            {
                ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);

                if (chessGame == null)
                    return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

                chessGame.Join(command.PlayerId);

                await _gameRepository.UpdateAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Games.StateError", ex.Message);
            }

        }
    }
}