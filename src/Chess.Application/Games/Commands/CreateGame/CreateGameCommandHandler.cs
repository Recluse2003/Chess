using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using MediatR;

namespace Chess.Application.Games.Commands.CreateGame
{
    /// <summary>
    /// Handles the execution of a player's <see cref="CreateGameCommand"/> request.
    /// Validates that the player attempting to do so exists, and persists the new <see cref="ChessGame"/> to the database.
    /// </summary>
    public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Result<Guid>>
    {
        private readonly IChessGameRepository _gameRepository;

        public CreateGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="CreateGameCommand"/>. Ensures that the player attempting create a 
        /// <see cref="ChessGame"/> exists.
        /// </summary>
        /// <param name="command">The details required to create a new <see cref="ChessGame"/>.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful create game request will provide a <see cref="Result"/> stating so, and containing the identifier
        /// of the new <see cref="ChessGame"/>. A failure will provide a <see cref="Result"/> containing an 
        /// <see cref="Error"/> stating the reason.
        /// </returns>
        public async Task<Result<Guid>> Handle(CreateGameCommand command, CancellationToken cancellationToken)
        {
            if (command.WhitePlayerId == string.Empty)
                return Error.Validation("Games.InvalidPlayer", "White Player ID cannot be empty.");

            try
            {
                ChessGame chessGame = new(Guid.NewGuid(), command.WhitePlayerId, FenConverterService.StartingPositionFen);

                await _gameRepository.AddAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return chessGame.Id;
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Games.StateError", ex.Message);
            }
        }
    }
}
