using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;
using Dapper;

namespace CartAPI.HealthChecks
{
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        public SqliteHealthCheck(IConfiguration config) => _config = config;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _config.GetConnectionString("DefaultConnection")
                    ?? "Data Source=app.db";

                using var conn = new SqliteConnection(connectionString);
                await conn.OpenAsync(cancellationToken);

                // Ejecuta una consulta súper liviana para comprobar que el motor de la DB responde
                await conn.ExecuteScalarAsync<int>("SELECT 1");

                return HealthCheckResult.Healthy("SELECT 1 ejecutado OK en Cart API");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    description: "No se pudo conectar a la base de datos SQLite de Cart API",
                    exception: ex);
            }
        }
    }
}