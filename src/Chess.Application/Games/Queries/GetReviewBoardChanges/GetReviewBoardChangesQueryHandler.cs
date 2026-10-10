using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using MediatR;

namespace Chess.Application.Games.Queries.GetReviewBoardChanges
{
    /// <summary>
    /// Handles the execution of a user's <see cref="GetReviewBoardChangesQuery"/> request. Validates the user is or was a player
    /// of the specified <see cref="ChessGame"/>.
    /// </summary>
    public class GetReviewBoardChangesQueryHandler : IRequestHandler<GetReviewBoardChangesQuery, Result<List<BoardChange>>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetReviewBoardChangesQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetReviewBoardChangesQuery"/>. Determines the differences between the current move 
        /// position, and the specified move position. Verifies that the user is or was a player of the specified 
        /// <see cref="ChessGame"/>.
        /// </summary>
        /// <param name="request">The details required to retrieve the <see cref="BoardChange"/>s.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation early, if required.</param>
        /// <returns>
        /// A successful request will provide a <see cref="Result"/> containing a read only list of <see cref="BoardChange"/>s
        /// between the two specified move positions. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/>
        /// stating the reason why.
        /// </returns>
        public async Task<Result<List<BoardChange>>> Handle(GetReviewBoardChangesQuery request, CancellationToken cancellationToken)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(request.GameId);

            if (game == null)
                return Error.NotFound("Games.NotFound", "The requested chess game was not found.");

            if (game.WhitePlayerId != request.UserId && game.BlackPlayerId != request.UserId)
                return Error.Unauthorized("Games.NotPlayer", "You are a not a player of the specified game.");

            if (request.CurrentMove < 0 || request.CurrentMove > game.MoveHistory.Count)
                return Error.Failure("Games.InvalidMove", "The current move is outside the valid range for this game.");

            if (request.TargetMove < 0 || request.TargetMove > game.MoveHistory.Count)
                return Error.Failure("Games.InvalidMove", "The requested move is outside the valid range for this game.");

            Board currentBoard = request.CurrentMove == 0 ? FenConverterService.FromFen(game.InitialFen)
                                                          : FenConverterService.FromFen(game.MoveHistory[request.CurrentMove - 1].FenAfterMove!);
            
            Board targetBoard = request.TargetMove == 0 ? FenConverterService.FromFen(game.InitialFen)
                                                        : FenConverterService.FromFen(game.MoveHistory[request.TargetMove - 1].FenAfterMove!);

            List<BoardChange> boardChanges = BoardComparerService.Compare(currentBoard, targetBoard);

            return boardChanges;
        }
    }
}
