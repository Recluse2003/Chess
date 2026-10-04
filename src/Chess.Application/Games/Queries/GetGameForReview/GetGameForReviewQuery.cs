using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Queries.GetGameForReview
{
    public record GetGameForReviewQuery(Guid GameId, string UserId) : IRequest<Result<ChessGame>>;
}
