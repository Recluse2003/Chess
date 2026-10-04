using Chess.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Queries.GetGamesByUserId
{
    public record GetGamesByUserIdQuery(string UserId, int PageNumber, int PageSize) : IRequest<Result<PagedList<GameDto>>>;
}
