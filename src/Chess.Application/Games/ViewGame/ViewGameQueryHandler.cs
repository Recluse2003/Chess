using Chess.Application.Games.MakeMove;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Games.ViewGame
{
    public class ViewGameQueryHandler
    {
        private readonly IChessGameRepository _gameRepository;

        public ViewGameQueryHandler(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<ViewGameDto?> ExecuteAsync(ViewGameQuery query)
        {
            ChessGame? game = await _gameRepository.GetByIdAsync(query.GameId);

            if (game == null)
                return null;

            bool isPlayer = game.WhitePlayerId == query.CurrentUserId || game.BlackPlayerId == query.CurrentUserId;

            if (!isPlayer)
                return null;

            ViewGameDto result = new ViewGameDto
            {
                GameId = game.Id,
                WhitePlayerId = game.WhitePlayerId,
                BlackPlayerId = game.BlackPlayerId,
                IsWhitePlayer = game.WhitePlayerId == query.CurrentUserId,
                Status = game.Status,
                CurrentTurn = game.Board.IsWhiteTurn
                ? "White"
                : "Black",
                Pieces = RetrievePieces(game)
            };

            return result;
        }

        private static IReadOnlyList<PieceDto> RetrievePieces(ChessGame game)
        {
            var pieces = new List<PieceDto>();

            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    var position = new Position(file, rank);
                    char piece = game.Board.GetPiece(position);

                    if (piece == '.')
                        continue;

                    pieces.Add(new PieceDto
                    {
                        File = file,
                        Rank = rank,
                        Piece = piece
                    });
                }
            }

            return pieces;
        }
    }
}
