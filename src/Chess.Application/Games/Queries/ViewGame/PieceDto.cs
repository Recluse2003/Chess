using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Queries.ViewGame
{
    public record PieceDto
    {
        public int File { get; init; }
        public int Rank { get; init; }
        public char Piece { get; init; }
    }
}
