using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using MediatR;

namespace Chess.Application.Games.Commands.JoinGame
{
    public class JoinGameCommandHandler : IRequestHandler<JoinGameCommand, Result>
    {
        private readonly IChessGameRepository _gameRepository;

        public JoinGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result> Handle(JoinGameCommand command, CancellationToken cancellationToken)
        {
            try
            {
                ChessGame? chessGame = await _gameRepository.GetByIdAsync(command.GameId);

                if (chessGame == null)
                    return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

                chessGame.Join(command.PlayerId);

                await _gameRepository.UpdateAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Games.StateError", ex.Message);
            }

        }
    }
}