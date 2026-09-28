using Chess.Application.Games.Commands.RemoveAbandonedGames;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chess.Infrastructure.BackgroundTasks
{
    public class RemoveAbandonedGamesWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RemoveAbandonedGamesWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

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
