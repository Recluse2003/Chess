using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.ForfeitExpiredGames;
using Chess.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chess.Infrastructure.BackgroundTasks
{
    public class GameTimeoutWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public GameTimeoutWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

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
