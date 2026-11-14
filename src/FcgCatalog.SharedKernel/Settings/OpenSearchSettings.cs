namespace FcgCatalog.SharedKernel.Settings;

/// <summary>
/// Configurações de acesso ao motor de busca (OpenSearch/Elasticsearch).
/// Em produção, Url/Username/Password devem vir de Secrets, nunca do código.
/// </summary>
public sealed class OpenSearchSettings
{
    public const string SectionName = "OpenSearch";

    public string Url { get; set; } = "http://localhost:9200";

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string IndexName { get; set; } = "fcg-games";

    /// <summary>
    /// Réplicas do índice. Use 0 em clusters de um único nó para manter o status green.
    /// </summary>
    public int NumberOfReplicas { get; set; }
}
