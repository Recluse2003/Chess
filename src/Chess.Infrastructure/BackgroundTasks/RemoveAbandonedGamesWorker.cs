using Chess.Application.Games.Commands.RemoveAbandonedGames;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chess.Infrastructure.BackgroundTasks
{
    /// <summary>
    /// Background service that periodically removes abandoned chess games.
    /// </summary>
    /// <remarks>
    /// A new dependency injection scope is created for each execution so that scoped services used by the command are
    /// disposed of after processing. The worker continues running until the application begins shutting down.
    /// </remarks>
    public class RemoveAbandonedGamesWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RemoveAbandonedGamesWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Runs the background loop that periodically removes abandoned games.
        /// </summary>
        /// <param name="stoppingToken">
        /// Token signalled when the host requests that the background service stop.
        /// </param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new RemoveAbandonedGamesCommand(), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
