using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;

namespace Chess.Application.Games.Commands.CreateGame
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

            chessGame.Join("whitePlayer"); // Temp addition to allow me to play chess from a single view for testing. 
            chessGame.Start("whitePlayer");

            await _gameRepository.AddAsync(chessGame);
            await _gameRepository.SaveChangesAsync();

            return chessGame.Id;
        }
    }
}
