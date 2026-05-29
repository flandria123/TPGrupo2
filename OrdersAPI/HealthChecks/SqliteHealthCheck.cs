using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.API.HealthCheck
{
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;

        public SqliteHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Busca la cadena de conexión en appsettings.json, si no la encuentra usa "Data Source=app.db" por defecto
                var connectionString = _config.GetConnectionString("DefaultConnection")
                                       ?? "Data Source=app.db";

                using var conn = new SqliteConnection(connectionString);
                await conn.OpenAsync(cancellationToken);

                // Ejecuta un comando ultra rápido y liviano para validar que la DB está viva
                var command = conn.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy("Conexión a SQLite y SELECT 1 ejecutados OK");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    description: "No se pudo establecer conexión con la base de datos SQLite.",
                    exception: ex);
            }
        }
    }
}
