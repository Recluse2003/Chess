using Chess.Application.Common.Results;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.GetGameForReview
{
    public class GetGameForReviewQueryHandler : IRequestHandler<GetGameForReviewQuery, Result<ChessGame>>
    {
        private readonly IChessGameRepository _chessGameRepository;

        public GetGameForReviewQueryHandler(IChessGameRepository chessGameRepository)
        {
            _chessGameRepository = chessGameRepository;
        }

        public Task<Result<ChessGame>> Handle(GetGameForReviewQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
