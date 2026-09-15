using Chess.Domain.Entities;
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

    public async Task<ChessGame> ExecuteAsync(MakeMoveCommand command)
    {
        ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

        if (game == null)
            throw new InvalidOperationException("Game not found.");

        Move move = new Move(Guid.NewGuid(), Position.FromChessNotation(command.From), Position.FromChessNotation(command.To), command.PromotionPiece);

        if (!_rules.IsMoveLegal(game.Board, move))
            throw new InvalidOperationException("Move is not legal.");

        game.MakeMove(move);

        game.CheckGameState(_rules);

        await _gameRepository.UpdateAsync(game);
        await _gameRepository.SaveChangesAsync();

        return game;
    }
}