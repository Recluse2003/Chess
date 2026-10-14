namespace Chess.Application.Interfaces
{
    /// <summary> 
    /// Provides persistence operations for <see cref="GameCodeEntity">. 
    /// </summary>
    public interface IGameCodeRepository
    {
        /// <summary>
        /// Removes the <see cref="GameCodeEntity" /> associated with a <see cref="ChessGame" />.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame" /> whose <see cref="GameCodeEntity" />
        /// is being removed.</param>
        Task DeleteCodeByGameIdAsync(Guid gameId);

        /// <summary>
        /// Retrieves the <see cref="GameCodeEntity" /> associated with a <see cref="ChessGame" />.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/>.</param>
        /// <returns>
        /// The join code connected to the specified <see cref="ChessGame"/>. Returns <see langword="null"/> if
        /// no <see cref="GameCodeEntity"/> exists.
        /// </returns>
        Task<string?> GetCodeByGameIdAsync(Guid gameId);
    }
}
