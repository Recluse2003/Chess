using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.ForfeitExpiredGames;
using Chess.Application.Games.Queries.GetGamesByUserId;
using Chess.Application.Games.Queries.GetRandomPublicGames;
using Chess.Application.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
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
            entity.DisconnectedPlayerId = game.DisconnectedPlayerId;
            entity.ReconnectDeadline = game.ReconnectDeadline;
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
        /// Deletes games that were created before the specified cutoff time and have not progressed beyond the waiting 
        /// for opponent or not started state.
        /// </summary>
        /// <param name="cutoffTime">The time before which waiting or not started games should be deleted.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous database operation early, if required.</param>
        /// <returns>
        /// A task representing the asynchronous delete operation.
        /// </returns>
        public async Task DeleteUnstartedGamesOlderThanAsync(DateTime cutoffTime, CancellationToken cancellationToken)
        {
            await _context.ChessGames
                .Where(g => g.CreatedAt < cutoffTime &&
                    (g.Status == GameStatus.WaitingForOpponent ||
                     g.Status == GameStatus.NotStarted))
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves the ID of the most recently created active or paused game associated with the specified user. Sets 
        /// games older than the most recent to abandoned, under the assumption that the player is attempting to play two games
        /// at once, which isn't allowed
        /// </summary>
        /// <param name="userId">The id of the user whose active or paused game should be retrieved.</param>
        /// <returns>
        /// The id of the user's most recently updated active or paused game, or <see langword="null"/> if no such game exists.
        /// </returns>
        public async Task<Guid?> GetActiveGameByUserIdAsync(string userId)
        {
            Guid? newestGameId = await _context.ChessGames
                .Where(g =>
                    (g.WhitePlayerId == userId || g.BlackPlayerId == userId) &&
                    (g.Status == GameStatus.Active || g.Status == GameStatus.Paused))
                .OrderByDescending(g => g.UpdatedAt)
                .Select(g => (Guid?)g.Id)
                .FirstOrDefaultAsync();
            /*
            List<ChessGameEntity> duplicateGames = await _context.ChessGames
                .Where(g => (g.WhitePlayerId == userId || g.BlackPlayerId == userId)
                            && (g.Status == GameStatus.Active || g.Status == GameStatus.Paused)
                            && g.Id != newestGameId)
                .ToListAsync();

            if (duplicateGames.Any())
            {
                foreach (ChessGameEntity duplicateGame in duplicateGames)
                {
                    duplicateGame.Status = GameStatus.Abandoned;
                }

                await _context.SaveChangesAsync();
            } 
            */
            return newestGameId;
        }

        /// <summary>
        /// Retrieves a random selection of public <see cref="ChessGame"/>s that are waiting for an opponent, excluding games 
        /// created by the specified user.
        /// </summary>
        /// <param name="userId">The id of the current user looking for public games. Id is used to exclude games that were 
        /// created by them.</param>
        /// <param name="count">The maximum number of public games to retrieve.</param>
        /// <param name="cancellationToken">A token used to end the asynchronous database operation early, if required.</param>
        /// <returns>
        /// A list of public games waiting for an opponent, with each result containing the game's join code and the 
        /// username of the player who created the game.
        /// </returns>
        public async Task<List<PublicGameDto>> GetRandomPublicGamesAsync(string userId, int count, CancellationToken cancellationToken)
        {
            return await _context.ChessGames
                .Where(g =>
                    !g.IsPrivate &&
                    g.Status == GameStatus.WaitingForOpponent &&
                    g.WhitePlayerId != null && 
                    g.WhitePlayerId != userId &&
                    g.GameCode != null)
                .OrderBy(g => Guid.NewGuid())
                .Take(count)
                .Select(g => new PublicGameDto
                {
                    Code = g.GameCode!.Code,
                    WhitePlayerUsername = g.WhitePlayer!.UserName!
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a paginated list of <see cref="ChessGame"/>s associated with the specified user,
        /// excluding games that are still waiting for an opponent or have not started.
        /// </summary>
        /// <param name="userId">The id of the user whose games are retrieved for.</param>
        /// <param name="pageNumber">The page number to retrieve. Values less than 1 are treated as page 1.</param>
        /// <param name="pageSize">The number of games to retrieve per page. Values less than 1 default to 10.</param>
        /// <returns>
        /// A <see cref="PagedList{T}"/> containing the user's games for the requested page, ordered from newest to 
        /// oldest by creation date.
        /// </returns>
        public async Task<PagedList<GameDto>> GetGamesByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.ChessGames
                .AsNoTracking()
                .Where(g => 
                    (g.WhitePlayerId == userId || g.BlackPlayerId == userId) &&
                     g.Status != GameStatus.WaitingForOpponent &&
                     g.Status != GameStatus.NotStarted);

            int totalCount = await query.CountAsync();

            IReadOnlyList<GameDto> games = await query
                .OrderByDescending(g => g.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GameDto
                {
                    Id = g.Id,
                    OpponentUsername = g.WhitePlayerId == userId
                        ? (g.BlackPlayer != null ? g.BlackPlayer.UserName ?? "Unknown" : "Unknown")
                        : (g.WhitePlayer != null ? g.WhitePlayer.UserName ?? "Unknown" : "Unknown"),
                    MoveCount = g.Moves.Count(),
                    WasWhitePlayer = g.WhitePlayerId == userId,
                    Status = g.Status,
                    EndReason = g.EndReason
                })
                .ToListAsync();

            return new PagedList<GameDto>(games, pageNumber, pageSize, totalCount);
        }

        /// <summary>
        /// Ends games by forfeit if a player stays disconnected over a specified deadline without reconnecting.
        /// </summary>
        /// <param name="cancellationToken">Triggers if a request ends early.</param>
        /// <returns>
        /// A list of <see cref="ExpiredGameDto"/>, which each contains the id of a <see cref="ChessGame"/> that was ended
        /// by forfeit, and which colour won. 
        /// </returns>
        public async Task<List<ExpiredGameDto>> ForfeitExpiredGamesAsync(CancellationToken cancellationToken)
        {
            List<ChessGameEntity> expiredGames = await _context.ChessGames
                .Where(g =>
                    g.Status == GameStatus.Paused &&
                    g.ReconnectDeadline != null && 
                    g.ReconnectDeadline <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            List<ExpiredGameDto> expiredGameResults = [];

            foreach (ChessGameEntity expiredGame in expiredGames)
            {
                if (expiredGame.DisconnectedPlayerId == expiredGame.WhitePlayerId)
                {
                    expiredGame.Status = GameStatus.BlackWin;
                    expiredGameResults.Add(new ExpiredGameDto(expiredGame.Id, false));
                }
                else if (expiredGame.DisconnectedPlayerId == expiredGame.BlackPlayerId)
                { 
                    expiredGame.Status = GameStatus.WhiteWin;
                    expiredGameResults.Add(new ExpiredGameDto(expiredGame.Id, true));
                }
                else
                {
                    continue;
                }

                expiredGame.EndReason = GameEndReason.Disconnect;
                expiredGame.DisconnectedPlayerId = null;
                expiredGame.ReconnectDeadline = null;
            }

            return expiredGameResults;
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
