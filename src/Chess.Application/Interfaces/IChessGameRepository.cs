using Chess.Domain.Entities;

namespace Chess.Application.Interfaces
{
    public interface IChessGameRepository
    {
        Task<ChessGame?> GetByIdAsync(Guid id);
        Task<string> CreateAsync(ChessGame game);
        Task UpdateAsync(ChessGame game);
        Task SaveChangesAsync();
    }
}
