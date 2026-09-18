using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;

namespace Chess.Application.Games.Commands.MakeMove;

public class MakeMoveCommandHandler
{
    private readonly IChessGameRepository _gameRepository;
    private readonly ChessRulesService _rules;

    public MakeMoveCommandHandler(IChessGameRepository gameRepository, ChessRulesService rules)
    {
        _gameRepository = gameRepository;
        _rules = rules;
    }

    public async Task<MoveResultDto> ExecuteAsync(MakeMoveCommand command)
    {
        ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

        if (game == null)
            throw new InvalidOperationException("Game not found.");

        if (game.Status != GameStatus.Active)
            throw new InvalidOperationException("Game not currently active.");

        // if (!game.CanPlayerMove(command.PlayerId))
        //    throw new InvalidOperationException("Currently not this player's turn.");

        Move move = new Move(
            Guid.NewGuid(),
            command.From,
            command.To,
            command.PromotionPiece
        );

        if (!_rules.IsMoveLegal(game.Board, move))
            throw new InvalidOperationException("Provided move is not legal.");

        IReadOnlyList<BoardChange> boardChanges = game.MakeMove(move);

        game.CheckGameState(_rules);

        await _gameRepository.UpdateAsync(game);
        await _gameRepository.SaveChangesAsync();

        return new MoveResultDto(true, boardChanges, game.Status, game.EndReason);
    }
}