using Chess.Domain.Entities;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using Chess.Infrastructure.Persistence.Models;

namespace Chess.Infrastructure.Persistence.Mappers
{
    public static class ChessGameMapper
    {
        public static ChessGameEntity ToEntity(ChessGame game)
        {
            return new ChessGameEntity
            {
                Id = game.Id,
                WhitePlayerId = game.WhitePlayerId,
                BlackPlayerId = game.BlackPlayerId,
                InitialFen = game.InitialFen,
                Fen = FenConverterService.ToFen(game.Board),
                Status = game.Status,
                EndReason = game.EndReason,
                UpdatedAt = DateTime.UtcNow,

                Moves = game.MoveHistory
                    .Select((move, index) => new MoveEntity
                    {
                        Id = move.Id,
                        GameId = game.Id,
                        MoveNumber = index + 1,
                        From = move.From.ToChessNotation(),
                        To = move.To.ToChessNotation(),
                        PromotionPiece = move.PromotionPiece,
                        FenAfterMove = move.FenAfterMove!,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList()
            };
        }

        public static ChessGame ToDomain(ChessGameEntity entity)
        {
            var game = ChessGame.Rehydrate(
                entity.Id,
                entity.WhitePlayerId,
                entity.BlackPlayerId,
                entity.InitialFen,
                entity.Fen,
                entity.Status,
                entity.EndReason,
                MoveMapper.ToDomainList(entity.Moves));

            return game;
        }
    }
}
