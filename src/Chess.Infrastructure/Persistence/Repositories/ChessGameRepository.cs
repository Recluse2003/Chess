using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using Chess.Infrastructure.Persistence.Mappers;
using Chess.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Allows <see cref="ChessGame"/> to be persisted using Entity Framework Core.
    /// </summary>
    /// <remarks> 
    /// <see cref="ChessGame"/> is converted into <see cref="ChessGameEntity"/> when being persisted, 
    /// as storing the <see cref="Board"/> would be ineffective. <see cref="Board"/> is converted into 
    /// FEN, which is then stored in <see cref="ChessGameEntity"/>.
    /// </remarks>
    public class ChessGameRepository : IChessGameRepository
    {
        private readonly ChessDbContext _context;

        public ChessGameRepository(ChessDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a <see cref="ChessGame"/> by it unique identifier.
        /// </summary>
        /// <param name="id">The id of the <see cref="ChessGame"/>.</param>
        /// <returns>
        /// The expected <see cref="ChessGame"/> if it exists, otherwise, returns <see langword="null"/>.
        /// </returns>
        public async Task<ChessGame?> GetByIdAsync(Guid id)
        {
            ChessGameEntity? entity = await _context.ChessGames
                .Include(game => game.Moves)
                .SingleOrDefaultAsync(game => game.Id == id);

            if (entity == null)
                return null;

            return ChessGameMapper.ToDomain(entity);
        }

        /// <summary>
        /// Retrieves the <see cref="ChessGame"/>'s unique identifier, with the join code associated with it.
        /// </summary>
        /// <param name="joinCode">The join code used to identify the <see cref="ChessGame"/>.</param>
        /// <returns>The unique identifier of the associated game when the code exists, otherwise, returns 
        /// <see langword="null"/>.</returns>
        public async Task<Guid?> GetGameIdByCodeAsync(string joinCode)
        {
            GameCodeEntity? entity = await _context.GameCodes
                .Include(code => code.ChessGame)
                .SingleOrDefaultAsync(code => code.Code == joinCode);

            return entity != null ? entity.ChessGame.Id 
                                  : null;
        }

        /// <summary>
        /// Adds a new <see cref="ChessGame"/> and generates a unique <see cref="GameCodeEntity"/> for it.
        /// </summary>
        /// <param name="game">The <see cref="ChessGame"/> to persist.</param>
        /// <returns>The unique join code generated for the game.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a unique lobby code cannot be generated after the maximum number of attempts.
        /// </exception>
        /// <remarks> 
        /// The game and its lobby code are added to the current database context but are not persisted 
        /// until <see cref="SaveChangesAsync"/> is called. 
        /// </remarks>
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

        /// <summary>
        /// Updates the persisted state of an existing <see cref="ChessGame"/>.
        /// </summary>
        /// <param name="game">The <see cref="ChessGame"/> containing the updated state.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified <see cref="ChessGame"/> does not exist.
        /// </exception>
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

        /// <summary>
        /// Persists all pending changes in the current database context.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
