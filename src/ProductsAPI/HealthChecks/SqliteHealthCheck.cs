using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProductsAPI.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqliteHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            await using var connection =
                new SqliteConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["database"] = "SQLite",
                ["service"] = "Products API"
            };

            return HealthCheckResult.Healthy(
                description:
                    "Conexión SQLite de Products API operativa.",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                description:
                    "Error de conexión con SQLite en Products API.",
                exception: ex);
        }
    }
}