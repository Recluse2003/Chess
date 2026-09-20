namespace Chess.Application.Interfaces
{
    public interface IGameCodeRepository
    {
        // Returns the Game ID linked to a code and removes the code atomically
        Task<Guid?> ConsumeCodeAsync(string code);
        Task<string?> GetCodeByGameIdAsync(Guid gameId);
    }
}
