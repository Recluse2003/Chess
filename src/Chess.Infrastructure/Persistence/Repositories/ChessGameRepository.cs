using Chess.Application.Interfaces;
using Chess.Domain.Entities;
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

        public async Task<Guid?> GetGameIdByCodeAsync(string joinCode)
        {
            GameCodeEntity? entity = await _context.GameCodes
                .Include(code => code.ChessGame)
                .SingleOrDefaultAsync(code => code.Code == joinCode);

            if (entity == null)
                return null;

            return entity != null ? entity.ChessGame.Id 
                                  : null;
        }

        public async Task<string> CreateAsync(ChessGame game)
        {
            ChessGameEntity gameEntity = ChessGameMapper.ToEntity(game);

            _context.ChessGames.Add(gameEntity);

            for (int attempt = 0; attempt < 10; attempt++)
            {
                string code = GameCodeEntity.Generate(5);

                bool exists = await _context.GameCodes.AnyAsync(x => x.Code == code);

                if (exists)
                    continue;

                GameCodeEntity codeEntity = new()
                {
                    Code = code,
                    ChessGameId = game.Id
                };

                _context.GameCodes.Add(codeEntity);

                return code;
            }

            throw new InvalidOperationException("Failed to generate a unique lobby code.");
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
