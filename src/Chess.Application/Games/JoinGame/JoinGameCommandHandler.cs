using Chess.Application.Games.CreateGame;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;

namespace Chess.Application.Games.JoinGame
{
    public class JoinGameCommandHandler
    {
        private readonly IChessGameRepository _gameRepository;

        public JoinGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Guid> ExecuteAsync(JoinGameCommand command)
        {
            ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);
            
            if (chessGame == null)
                throw new InvalidOperationException("Game not found.");

            chessGame.Join(command.BlackPlayerId);

            await _gameRepository.UpdateAsync(chessGame);
            await _gameRepository.SaveChangesAsync();

            return command.GameId;
        }
    }
}
