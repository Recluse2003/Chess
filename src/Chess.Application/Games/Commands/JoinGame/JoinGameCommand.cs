using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Commands.JoinGame
{
    public record JoinGameCommand(Guid GameId, string PlayerId);
}
