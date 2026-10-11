using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetActiveLobbyByUserId
{
    /// <summary>
    /// Handles the execution of a <see cref="GetActiveLobbyByUserIdQuery"/> request.
    /// Validates that a <see cref="ChessGame"/> in the lobby with that user id exists.
    /// </summary>
    public class GetActiveLobbyByUserIdQueryHandler : IRequestHandler<GetActiveLobbyByUserIdQuery, Result<Guid>>
    {
        private readonly IChessGameRepository _gameRepository;

        public GetActiveLobbyByUserIdQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Processes the incoming <see cref="GetActiveLobbyByUserIdQuery"/>. Validates that a <see cref="ChessGame"/> with
        /// that user id exists, and that it is currently still in the lobby.
        /// </summary>
        /// <param name="query">Contains the details required to perform the request.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous database operation early, if required.</param>
        /// <returns>
        /// A successful get active lobby request will provide a <see cref="Result"/> stating so, which contains the id of the 
        /// <see cref="ChessGame"/> in the lobby. A failure will provide a <see cref="Result"/> containing an <see cref="Error"/> 
        /// stating the reason why.
        /// </returns>
        public async Task<Result<Guid>> Handle(GetActiveLobbyByUserIdQuery query, CancellationToken cancellationToken)
        {
            Guid? chessGameId = await _gameRepository.GetActiveLobbyByUserIdAsync(query.UserId);

            if (chessGameId == null)
                return Error.NotFound("Games.ActiveLobbyNotFound", "The provided user Id is not linked to any chess game in the lobby.");

            return chessGameId;
        }
    }
}
