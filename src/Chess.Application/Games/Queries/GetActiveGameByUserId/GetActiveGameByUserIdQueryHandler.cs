using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetActiveGameByUserId
{
    /// <summary>
    /// Handles the execution of a <see cref="GetActiveGameByUserIdQuery"/> request.
    /// Validates that an active <see cref="ChessGame"/> with that user id exists.
    /// </summary>
    public class GetActiveGameByUserIdQueryHandler : IRequestHandler<GetActiveGameByUserIdQuery, Result<Guid>>
    {
        private readonly IChessGameRepository _chessGameRepository;

        public GetActiveGameByUserIdQueryHandler(IChessGameRepository chessGameRepository)
        {
            _chessGameRepository = chessGameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetActiveGameByUserIdQuery"/>. Validates that a <see cref="ChessGame"/> with
        /// that user id exists, and that it is currently active.
        /// </summary>
        /// <param name="query">Contains the details required to perform the request.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous database operation early, if required.</param>
        /// <returns>
        /// A successful get active game request will provide a <see cref="Result"/> stating so, which contains the id of the 
        /// active <see cref="ChessGame"/>. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> 
        /// stating the reason why.
        /// </returns>
        public async Task<Result<Guid>> Handle(GetActiveGameByUserIdQuery query, CancellationToken cancellationToken)
        {
            Guid? chessGameId = await _chessGameRepository.GetActiveGameByUserIdAsync(query.UserId);

            if (chessGameId == null)
                return Error.NotFound("Games.ActiveGameNotFound", "The provided user Id is not linked to any active chess game");

            return chessGameId;
        }
    }
}
