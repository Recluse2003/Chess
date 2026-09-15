using Chess.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.Queries.ViewGame
{
    public record ViewGameDto
    {
        public Guid GameId { get; init; }
        public string WhitePlayerId { get; init; } = string.Empty;
        public string? BlackPlayerId { get; init; }
        public bool IsWhitePlayer { get; init; }
        public GameStatus Status { get; init; }
        public string CurrentTurn { get; init; } = string.Empty;
        public IReadOnlyList<PieceDto> Pieces { get; init; } = [];
    }
}
