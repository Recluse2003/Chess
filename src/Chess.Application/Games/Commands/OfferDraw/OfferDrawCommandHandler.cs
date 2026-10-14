using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.OfferDraw
{
    /// <summary>
    /// Handles the execution of a player's <see cref="OfferDrawCommand"/> request.
    /// Validates the <see cref="ChessGame"/> exists, that it is active, and that the requester is a player of the specified 
    /// <see cref="ChessGame"/>.
    /// </summary>
    public class OfferDrawCommandHandler : IRequestHandler<OfferDrawCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public OfferDrawCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="OfferDrawCommand">, ensures the specified <see cref="ChessGame"/> exists, it is 
        /// active, and that requester is a player of the <see cref="ChessGame"/>.
        /// </summary>
        /// <param name="command">The details of the draw offer.</param>
        /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
        /// <returns>
        /// A successful move execution will provide a <see cref="Result"/> A failure will provide a <see cref="Result"/> 
        /// containing an <see cref="Error"/> stating the reason why.
        /// </returns>
        public async Task<Result> Handle(OfferDrawCommand command, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

            if (game == null)
                return Error.NotFound("Games.GameNotFound", "The provided game could not be found.");

            try
            {
                game.OfferDraw(command.UserId);

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
            catch (GameStateTransitionException ex)
            {
                return Error.Conflict("Games.OfferDrawFailed", ex.Message);
            }
        }
    }
}
