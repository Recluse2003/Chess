using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.JoinGame
{
    public record JoinGameCommand(Guid GameId, string BlackPlayerId);
}
