using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.StartGame
{
    public class StartGameCommandHandler : IRequestHandler<StartGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public StartGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result> Handle(StartGameCommand command, CancellationToken token)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);

            if (chessGame == null)
                return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

            try
            {
                chessGame.Start(command.PlayerId);

                await _gameRepository.UpdateAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Games.StartGameFailure", ex.Message);
            }
        }
    }
}
