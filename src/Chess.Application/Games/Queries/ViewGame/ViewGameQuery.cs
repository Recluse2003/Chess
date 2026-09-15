using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Queries.ViewGame
{
    public record ViewGameQuery(Guid GameId, string CurrentUserId);
}
