using Chess.Domain.Entities;
using Chess.Domain.Services;
using Chess.Infrastructure.Persistence.Models;

namespace Chess.Infrastructure.Persistence.Mappers
{
    /// <summary>
    /// Provides mapping operations between the domain <see cref="ChessGame"/> and its persistence 
    /// representation <see cref="ChessGameEntity"/>.
    /// </summary>
    public static class ChessGameMapper
    {
        /// <summary>
        /// Converts <see cref="ChessGame"/> to <see cref="ChessGameEntity"/>.
        /// </summary>
        /// <param name="game">The <see cref="ChessGame"/> to be converted.</param>
        /// <returns>
        /// A <see cref="ChessGameEntity"/> containing the persisted representation of the supplied chess game.
        /// </returns>
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

        /// <summary>
        /// Converts <see cref="ChessGameEntity"/> to <see cref="ChessGame"/>.
        /// </summary>
        /// <param name="entity">The <see cref="ChessGameEntity"/> to be converted.</param>
        /// <returns>
        /// A rehydrated <see cref="ChessGame"/> representing the persisted state.
        /// </returns>
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
