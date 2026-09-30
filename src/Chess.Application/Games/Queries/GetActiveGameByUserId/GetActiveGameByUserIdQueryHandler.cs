using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Queries.GetActiveGameByUserId
{
    /// <summary>
    /// Handles the execution of a <see cref="GetActiveGameByUserIdQuery"/> request.
    /// Validates that a <see cref="ChessGame"/> with that user id exists, and that it is currently
    /// active.
    /// </summary>
    public class GetActiveGameByUserIdQueryHandler : IRequestHandler<GetActiveGameByUserIdQuery, Result<Guid>>
    {
        private readonly IChessGameRepository _chessGameRepository;

        public GetActiveGameByUserIdQueryHandler(IChessGameRepository chessGameRepository)
        {
            _chessGameRepository = chessGameRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<Guid>> Handle(GetActiveGameByUserIdQuery query, CancellationToken cancellationToken)
        {
            Guid? chessGameId = await _chessGameRepository.GetActiveGameByUserIdAsync(query.UserId);

            if (chessGameId == null)
                return Error.NotFound("Games.ActiveGameNotFound", "The provided userId is not linked to any active chess game");

            return chessGameId;
        }
    }
}
