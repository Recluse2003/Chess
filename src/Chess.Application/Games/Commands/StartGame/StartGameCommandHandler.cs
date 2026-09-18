using Chess.Application.Games.Commands.CreateGame;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;

namespace Chess.Application.Games.Commands.StartGame
{
    public class StartGameCommandHandler
    {
        private readonly IChessGameRepository _gameRepository;

        public StartGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<bool> ExecuteAsync(StartGameCommand command)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);

            if (chessGame == null)
                return false;

            chessGame.Start(command.PlayerId);

            await _gameRepository.UpdateAsync(chessGame);
            await _gameRepository.SaveChangesAsync();

            return true;
        }
    }
}
