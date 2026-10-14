using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.ForfeitExpiredGames;
using Chess.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chess.Infrastructure.BackgroundTasks
{
    /// <summary>
    /// Background service that is run periodically to check for expired chess games, which then notifies connected clients
    /// of expired games that they have been forfeited due to timeout. 
    /// </summary>
    /// <remarks>
    /// The worker creates a dependency injection scope for each check so that scoped services, such as the mediator and 
    /// repositories used by the command, are resolved and disposed of correctly.
    /// </remarks>
    public class GameTimeoutWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public GameTimeoutWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Runs the background loop that identifies and processes expired games.
        /// </summary>
        /// <param name="stoppingToken">
        /// Token signalled when the host requests that the background service stop.
        /// </param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("GameTimeoutWorker started!");

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("GameTimeoutWorker checking games...");

                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    IGameNotifier notifier = scope.ServiceProvider.GetRequiredService<IGameNotifier>();

                    Result<List<ExpiredGameDto>> expiredGames = await mediator.Send(new ForfeitExpiredGamesCommand(), stoppingToken);

                    foreach (ExpiredGameDto expiredGame in expiredGames.Value)
                    {
                        await notifier.GameEndedByForfeit(expiredGame.GameId, expiredGame.IsWhiteWinner);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
