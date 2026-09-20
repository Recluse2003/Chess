using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
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
        private readonly IGameCodeRepository _codeRepository;

        public JoinGameCommandHandler(IChessGameRepository gameRepository, IGameCodeRepository codeRepository)
        {
            _gameRepository = gameRepository;
            _codeRepository = codeRepository;
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
                Guid? chessGameId = await _codeRepository.ConsumeCodeAsync(command.Code);
                if (chessGameId == null)
                    return Error.NotFound("Games.InvalidCode", "The provided game code was not linked to any active games.");

                ChessGame? chessGame = await _gameRepository.GetByIdAsync(chessGameId.Value);
                if (chessGame == null)
                    return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

                chessGame.Join(command.PlayerId);

                await _gameRepository.UpdateAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (GameStateTransitionException ex)
            {
                return Error.Conflict("Games.JoinFailed", ex.Message);
            }

        }
    }
}