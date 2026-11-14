using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;

namespace FcgCatalog.Api.HealthChecks;

/// <summary>
/// Verifica a disponibilidade do cluster OpenSearch/Elasticsearch.
/// </summary>
public sealed class OpenSearchHealthCheck(IOpenSearchClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var response = await client.Cluster.HealthAsync(ct: cancellationToken);

        if (!response.IsValid)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "OpenSearch indisponível.",
                response.OriginalException);
        }

        return response.Status == OpenSearch.Net.HealthStatus.Red
            ? new HealthCheckResult(context.Registration.FailureStatus, "Cluster OpenSearch com status red.")
            : HealthCheckResult.Healthy($"Cluster OpenSearch com status {response.Status}.");
    }
}
