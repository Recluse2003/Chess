using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.PauseGame;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Exceptions;
using MediatR;

namespace Chess.Application.Games.Commands.ResumeGame
{
    public class PauseGameCommandHandler : IRequestHandler<PauseGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public PauseGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

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
