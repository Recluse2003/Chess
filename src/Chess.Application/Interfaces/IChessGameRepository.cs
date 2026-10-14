using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.ForfeitExpiredGames;
using Chess.Application.Games.Queries.GetGamesByUserId;
using Chess.Application.Games.Queries.GetRandomPublicGames;
using Chess.Domain.Entities;

namespace Chess.Application.Interfaces
{
    /// <summary>
    /// Allows <see cref="ChessGame"/> to be persisted using Entity Framework Core.
    /// </summary>
    /// <remarks> 
    /// <see cref="ChessGame"/> is converted into <see cref="ChessGameEntity"/> when being persisted, 
    /// as storing the <see cref="Board"/> would be ineffective. <see cref="Board"/> is converted into 
    /// FEN, which is then stored in <see cref="ChessGameEntity"/>.
    /// </remarks>
    public interface IChessGameRepository
    {
        /// <summary>
        /// Retrieves a <see cref="ChessGame"/> by it unique identifier.
        /// </summary>
        /// <param name="id">The id of the <see cref="ChessGame"/>.</param>
        /// <returns>
        /// The expected <see cref="ChessGame"/> if it exists, otherwise, returns <see langword="null"/>.
        /// </returns>
        Task<ChessGame?> GetByIdAsync(Guid id);

        /// <summary>
        /// Retrieves the <see cref="ChessGame"/>'s unique identifier, with the join code associated with it.
        /// </summary>
        /// <param name="joinCode">The join code used to identify the <see cref="ChessGame"/>.</param>
        /// <returns>The unique identifier of the associated game when the code exists, otherwise, returns 
        /// <see langword="null"/>.</returns>
        Task<Guid?> GetGameIdByCodeAsync(string joinCode);

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
        Task<string> CreateAsync(ChessGame game);

        /// <summary>
        /// Updates the persisted state of an existing <see cref="ChessGameEntity"/>.
        /// </summary>
        /// <param name="game">The <see cref="ChessGame"/> containing the updated state, to be mapped to an existing 
        /// <see cref="ChessGameEntity"/>.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <see cref="ChessGame"/> does not map to any existing <see cref="ChessGameEntity"/>.
        /// </exception>
        Task UpdateAsync(ChessGame game);

        /// <summary>
        /// Deletes the persisted state of an existing <see cref="ChessGame"/> through its id.
        /// </summary>
        /// <param name="gameId">Id of the <see cref="ChessGame"/> containing the updated state.</param>
        Task DeleteAsync(Guid gameId);

        /// <summary>
        /// Deletes games that were created before the specified cutoff time and have not progressed beyond the waiting 
        /// for opponent or not started state.
        /// </summary>
        /// <param name="cutoffTime">The time before which waiting or not started games should be deleted.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous database operation early, if required.</param>
        /// <returns>
        /// A task representing the asynchronous delete operation.
        /// </returns>
        Task DeleteUnstartedGamesOlderThanAsync(DateTime cutoffTime, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the ID of the most recently created active or paused game associated with the specified user. Sets 
        /// games older than the most recent to abandoned, under the assumption that the player is attempting to play two games
        /// at once, which isn't allowed
        /// </summary>
        /// <param name="userId">The id of the user whose active or paused game should be retrieved.</param>
        /// <returns>
        /// The id of the user's most recently updated active or paused game, or <see langword="null"/> if no such game exists.
        /// </returns>
        Task<Guid?> GetActiveGameByUserIdAsync(string userId);

        /// <summary>
        /// Retrieves the ID of the most recently created chess game in a lobby associated with the specified user. Sets 
        /// games older than the most recent to abandoned, under the assumption that the player is attempting to join two games
        /// at once, which isn't allowed.
        /// </summary>
        /// <param name="userId">The id of the user whose active or paused game should be retrieved.</param>
        /// <returns>
        /// The id of the user's most recently joined game still in the lobby, or <see langword="null"/> if no such game exists.
        /// </returns>
        Task<Guid?> GetActiveLobbyByUserIdAsync(string userId);

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
        Task<List<PublicGameDto>> GetRandomPublicGamesAsync(string userId, int count, CancellationToken cancellationToken);

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
        Task<PagedList<GameDto>> GetGamesByUserIdAsync(string userId, int pageNumber, int pageSize);

        /// <summary>
        /// Ends games by forfeit if a player stays disconnected over a specified deadline without reconnecting.
        /// </summary>
        /// <param name="cancellationToken">Triggers if a request ends early.</param>
        /// <returns>
        /// A list of <see cref="ExpiredGameDto"/>, which each contains the id of a <see cref="ChessGame"/> that was ended
        /// by forfeit, and which colour won. 
        /// </returns>
        Task<List<ExpiredGameDto>> ForfeitExpiredGamesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Persists all pending changes in the current database context.
        /// </summary>
        Task SaveChangesAsync();
    }
}
