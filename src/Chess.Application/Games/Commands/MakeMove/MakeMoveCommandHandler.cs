using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.Exceptions;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Commands.MakeMove;

/// <summary>
/// Handles the execution of a player's <see cref="MakeMoveCommand"/> request.
/// Validates game state, rule compliance, and updates the database.
/// </summary>
public class MakeMoveCommandHandler : IRequestHandler<MakeMoveCommand, Result<MoveResultDto>>
{
    private readonly IChessGameRepository _gameRepository;
    private readonly ChessRulesService _rules;
    private readonly SanNotationService _sanNotationService;

    public MakeMoveCommandHandler(IChessGameRepository gameRepository, ChessRulesService rules, SanNotationService sanNotationService) 
    {
        _gameRepository = gameRepository;
        _rules = rules;
        _sanNotationService = sanNotationService;
    }

    /// <summary>
    /// Processes the incoming <see cref="MakeMoveCommand">, enforces chess rules, and commit changes to the database.
    /// </summary>
    /// <param name="command">The details of the requested move.</param>
    /// <param name="cancellationToken">Triggers if the HTTP or network request is aborted early.</param>
    /// <returns>
    /// A successful move execution will provide a <see cref="Result"/> containing the <see cref="BoardChange"/>s, and new state 
    /// of the game. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> stating the reason.
    /// </returns>
    public async Task<Result<MoveResultDto>> Handle(MakeMoveCommand command, CancellationToken cancellationToken) 
    {
        ChessGame? game = await _gameRepository.GetByIdAsync(command.GameId);

        if (game == null)
            return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

        if (game.Status != GameStatus.Active)
            return Error.Conflict("Games.NotActive", "The game is not currently active.");
         
        if (!game.CanPlayerMove(command.PlayerId))
            return Error.Conflict("Games.NotYourTurn", "It is currently not your turn to move.");

        Move move = new Move(Guid.NewGuid(), command.From, command.To, command.PromotionPiece);

        if (!_rules.IsMoveLegal(game.Board, move))
            return Error.Validation("Games.IllegalMove", "The provided chess move is illegal.");

        try
        {
            // Apply the move once.
            BoardMoveResult moveResult = game.Board.ApplyMove(move);

            // Generate SAN using the original and resulting positions.
            string notation = _sanNotationService.Generate(game.Board, move, moveResult.Board);

            // Record the move and update the game's board.
            IReadOnlyList<BoardChange> boardChanges = game.MakeMove(move, moveResult, notation);

            game.CheckGameState(_rules);

            await _gameRepository.UpdateAsync(game);
            await _gameRepository.SaveChangesAsync();

            return new MoveResultDto(true, boardChanges, game.Board.IsWhiteTurn, game.Status, game.EndReason);
        }
        catch (GameNotActiveException ex)
        {
            return Error.Conflict("Games.NotActive", ex.Message);
        }
        catch (InvalidTurnException ex)
        {
            return Error.Conflict("Games.NotYourTurn", ex.Message);
        }
    }
}