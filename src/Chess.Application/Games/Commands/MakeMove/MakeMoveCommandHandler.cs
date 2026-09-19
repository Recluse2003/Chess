using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Commands.MakeMove;

public class MakeMoveCommandHandler : IRequestHandler<MakeMoveCommand, Result<MoveResultDto>>
{
    private readonly IChessGameRepository _gameRepository;
    private readonly ChessRulesService _rules;

    public MakeMoveCommandHandler(IChessGameRepository gameRepository, ChessRulesService rules) 
    {
        _gameRepository = gameRepository;
        _rules = rules;
    }

    public async Task<Result<MoveResultDto>> Handle(MakeMoveCommand command, CancellationToken cancellationToken) 
    {
        ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

        if (game == null)
            return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

        if (game.Status != GameStatus.Active)
            return Error.Conflict("Games.NotActive", "The game is not currently active.");

        // if (!game.CanPlayerMove(command.PlayerId))
        //    return Error.Conflict("Games.NotYourTurn", "It is currently not your turn to move.");

        Move move = new Move(
            Guid.NewGuid(),
            command.From,
            command.To,
            command.PromotionPiece
        );

        if (!_rules.IsMoveLegal(game.Board, move))
            return Error.Validation("Games.IllegalMove", "The provided chess move is illegal.");

        try
        {
            IReadOnlyList<BoardChange> boardChanges = game.MakeMove(move);

            game.CheckGameState(_rules);

            await _gameRepository.UpdateAsync(game);
            await _gameRepository.SaveChangesAsync();

            return new MoveResultDto(true, boardChanges, game.Status, game.EndReason);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict("Games.MoveExecutionFailed", ex.Message);
        }
    }
}