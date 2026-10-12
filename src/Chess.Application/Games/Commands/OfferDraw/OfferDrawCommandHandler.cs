using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.OfferDraw
{
    public class OfferDrawCommandHandler : IRequestHandler<OfferDrawCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public OfferDrawCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

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
