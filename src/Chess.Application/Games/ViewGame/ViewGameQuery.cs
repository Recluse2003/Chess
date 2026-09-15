using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.ViewGame
{
    public record ViewGameQuery(Guid GameId, string CurrentUserId);
}
