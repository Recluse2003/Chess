using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using MediatR;

namespace Chess.Application.Games.Commands.CreateGame
{
    public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Result<Guid>>
    {
        private readonly IChessGameRepository _gameRepository;

        public CreateGameCommandHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Result<Guid>> Handle(CreateGameCommand command, CancellationToken cancellationToken)
        {
            if (command.WhitePlayerId == string.Empty)
            {
                return Error.Validation("Games.InvalidPlayer", "White Player ID cannot be empty.");
            }

            try
            {
                ChessGame chessGame = new(Guid.NewGuid(), command.WhitePlayerId, FenConverterService.StartingPositionFen);

                await _gameRepository.AddAsync(chessGame);
                await _gameRepository.SaveChangesAsync();

                return chessGame.Id;
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Games.StateError", ex.Message);
            }
        }
    }
}
