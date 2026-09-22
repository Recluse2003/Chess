namespace Chess.Application.Interfaces
{
    public interface IGameCodeRepository
    {
        Task DeleteCodeByGameIdAsync(Guid gameId);
        Task<string?> GetCodeByGameIdAsync(Guid gameId);
    }
}
