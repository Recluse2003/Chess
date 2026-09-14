using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using Chess.Infrastructure.Persistence.Mappers;
using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories
{
    public class ChessGameRepository : IChessGameRepository
    {
        private readonly ChessDbContext _context;

        public ChessGameRepository(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<ChessGame?> GetByIdAsync(Guid id)
        {
            ChessGameEntity? entity = await _context.ChessGames
                .Include(game => game.Moves)
                .SingleOrDefaultAsync(game => game.Id == id);

            if (entity == null)
                return null;

            return ChessGameMapper.ToDomain(entity);
        }

        public async Task AddAsync(ChessGame game)
        {
            ChessGameEntity entity = ChessGameMapper.ToEntity(game);

            await _context.ChessGames.AddAsync(entity);
        }

        public async Task UpdateAsync(ChessGame game)
        {
            ChessGameEntity? entity = await _context.ChessGames
                .Include(g => g.Moves)
                .SingleOrDefaultAsync(g => g.Id == game.Id);

            if (entity is null)
                throw new InvalidOperationException("Game not found.");

            entity.WhitePlayerId = game.WhitePlayerId;
            entity.BlackPlayerId = game.BlackPlayerId;
            entity.InitialFen = game.InitialFen;
            entity.Fen = FenConverterService.ToFen(game.Board);
            entity.Status = game.Status;
            entity.EndReason = game.EndReason;
            entity.UpdatedAt = DateTime.UtcNow;

            var existingMoveIds = entity.Moves
                .Select(move => move.Id)
                .ToHashSet();

            foreach (Move move in game.MoveHistory)
            {
                if (existingMoveIds.Contains(move.Id))
                    continue;

                var newMove = new MoveEntity
                {
                    Id = move.Id,
                    GameId = game.Id,
                    MoveNumber = game.MoveHistory.IndexOf(move) + 1,
                    From = move.From.ToChessNotation(),
                    To = move.To.ToChessNotation(),
                    PromotionPiece = move.PromotionPiece,
                    FenAfterMove = move.FenAfterMove!,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Moves.Add(newMove);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
