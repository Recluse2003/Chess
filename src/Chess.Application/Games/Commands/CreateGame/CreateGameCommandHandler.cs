using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using MediatR;

namespace Chess.Application.Games.Commands.CreateGame
{
    /// <summary>
    /// Handles the execution of a player's <see cref="CreateGameCommand"/> request.
    /// Validates that the player attempting to do so exists, and persists the new <see cref="ChessGame"/> to the database.
    /// </summary>
    public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Result<CreateGameDto>>
    {
        private readonly IChessGameRepository _gameRepository;
        private readonly IGameCodeRepository _codeRepository;

        public CreateGameCommandHandler(IChessGameRepository gameRepository, IGameCodeRepository codeRepository)
        {
            _gameRepository = gameRepository;
            _codeRepository = codeRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="CreateGameCommand"/>. Ensures that the player attempting create a 
        /// <see cref="ChessGame"/> exists.
        /// </summary>
        /// <param name="command">The details required to create a new <see cref="ChessGame"/>.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful create game request will provide a <see cref="Result"/> stating so, which contains the identifier
        /// of the new <see cref="ChessGame"/>, and its join code. A failure will provide a <see cref="Result"/> containing an 
        /// <see cref="Error"/> stating the reason.
        /// </returns>
        public async Task<Result<CreateGameDto>> Handle(CreateGameCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.WhitePlayerId))
                return Error.Validation("Games.InvalidPlayer", "White Player ID cannot be empty.");

            try
            {
                ChessGame chessGame = new(Guid.NewGuid(), command.WhitePlayerId, FenConverterService.StartingPositionFen);

                string joinCode = await _gameRepository.CreateAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return new CreateGameDto(chessGame.Id, joinCode);
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Games.StateError", ex.Message);
            }
        }
    }
}
