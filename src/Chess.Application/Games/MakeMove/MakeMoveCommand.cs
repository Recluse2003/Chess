using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.MakeMove
{
    public record MakeMoveCommand(Guid GameId, string From, string To, char? PromotionPiece);
}
