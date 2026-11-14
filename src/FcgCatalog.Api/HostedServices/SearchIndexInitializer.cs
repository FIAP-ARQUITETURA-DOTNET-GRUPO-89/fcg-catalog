using FcgCatalog.Application.Commands.Games;
using MediatR;

namespace FcgCatalog.Api.HostedServices;

/// <summary>
/// Garante, na inicialização da API, que o índice de busca exista e esteja em sincronia com o
/// banco relacional, reconstruindo-o quando necessário. Tenta novamente até conseguir, para que
/// uma indisponibilidade do OpenSearch ou do PostgreSQL na subida não deixe o índice vazio.
/// </summary>
public sealed partial class SearchIndexInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<SearchIndexInitializer> logger)
    : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = InitialDelay;

        for (var attempt = 1; !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ReindexGamesCommand(OnlyIfOutOfSync: true), stoppingToken);

                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogAttemptFailed(logger, ex, attempt, delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
                delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, MaxDelay.Ticks));
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Falha ao sincronizar o índice de busca (tentativa {Attempt}). Nova tentativa em {DelaySeconds}s.")]
    private static partial void LogAttemptFailed(
        ILogger logger,
        Exception exception,
        int attempt,
        double delaySeconds);
}
