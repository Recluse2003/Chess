using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Commands.StartGame
{
    public record StartGameCommand(Guid GameId, string PlayerId);
}
