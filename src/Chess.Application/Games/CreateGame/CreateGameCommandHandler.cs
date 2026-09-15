using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Enums;
using Chess.Domain.Services;

namespace Chess.Application.Games.CreateGame
{
    public class CreateGameCommandHandler
    {
        private readonly IChessGameRepository _gameRepository;

        public CreateGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateGameCommand command)
        {
            ChessGame chessGame = new ChessGame(
                Guid.NewGuid(), 
                command.WhitePlayerId,
                FenConverterService.StartingPositionFen);

            await _gameRepository.AddAsync(chessGame);
            await _gameRepository.SaveChangesAsync();

            return chessGame.Id;
        }
    }
}
