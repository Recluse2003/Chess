using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.LeaveLobby
{
    /// <summary>
    /// Handles the execution of a player's <see cref="LeaveLobbyCommand"/> request.
    /// Validates the game exists, the requester is an actual player of the game, and that the game hasn't started yet. 
    /// To be used only for the black player leaving a lobby. 
    /// </summary>
    public class LeaveLobbyCommandHandler : IRequestHandler<LeaveLobbyCommand, Result<bool>>
    {
        private readonly IChessGameRepository _gameRepository;

        public LeaveLobbyCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="LeaveLobbyCommand"/>. Validate the <see cref="ChessGame"/> exists, the 
        /// requester is a player of the game and that the game is still waiting to start. If the player was the host, the game is 
        /// deleted, else the <see cref="ChessGame"/> allows a new player to join.
        /// </summary>
        /// <param name="command">The details needed to pause the game.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful leave lobby request will provide a <see cref="Result"/> stating so, along with a bool value to indicate if 
        /// the host left, if equal to true. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> 
        /// stating the reason why.
        /// </returns>
        public async Task<Result<bool>> Handle(LeaveLobbyCommand command, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

            if (game == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            if (game.Status != GameStatus.WaitingForOpponent && game.Status != GameStatus.NotStarted)
                return Error.Conflict("Games.AlreadyActive", "The game has already started.");

            // Host leaving closes the entire lobby.
            if (game.WhitePlayerId == command.UserId)
            {
                await _gameRepository.DeleteAsync(game.Id);
                await _gameRepository.SaveChangesAsync();

                return true;
            }
            else
            {
                // Opponent leaving removes them from the lobby.
                try
                {
                    game.Leave(command.UserId);

                    await _gameRepository.UpdateAsync(game);
                    await _gameRepository.SaveChangesAsync();

                    return false;
                }
                catch (NotPlayerException ex)
                {
                    return Error.Unauthorized("Games.NotPlayer", ex.Message);
                }
                catch (GameStateTransitionException ex)
                {
                    return Error.Conflict("Games.LeaveFailed", ex.Message);
                }
            }
        }
    }
}
