using Chess.Domain.Entities;

namespace Chess.Domain.Interfaces
{
    public interface IChessGameRepository
    {
        Task<ChessGame?> GetByIdAsync(Guid id);
        Task AddAsync(ChessGame game);
        Task UpdateAsync(ChessGame game);
        Task SaveChangesAsync();
    }
}
