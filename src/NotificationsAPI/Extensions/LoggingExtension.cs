namespace NotificationsAPI.Extensions;

using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()

            // ─────────────────────────────────────────────
            // NIVELES MÍNIMOS
            // ─────────────────────────────────────────────
            .MinimumLevel.Information()

            // Reducir ruido de ASP.NET Core
            .MinimumLevel.Override(
                "Microsoft",
                LogEventLevel.Warning)

            // Mantener logs HTTP relevantes
            .MinimumLevel.Override(
                "Microsoft.AspNetCore.Hosting.Diagnostics",
                LogEventLevel.Information)

            // Incluir propiedades del contexto
            // (ej: CorrelationId)
            .Enrich.FromLogContext()

            // ─────────────────────────────────────────────
            // CONSOLA
            // Solo errores reales
            // ─────────────────────────────────────────────
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le =>
                    le.Level >= LogEventLevel.Error)

                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))

            // ─────────────────────────────────────────────
            // ARCHIVO JSON ESTRUCTURADO
            // Auditoría principal del TP
            // ─────────────────────────────────────────────
            .WriteTo.Logger(lc => lc

                .Filter.ByIncludingOnly(le =>
                {
                    // Solo logs HTTP del middleware oficial
                    var isRequestLog =
                        Matching.FromSource(
                            "Serilog.AspNetCore.RequestLoggingMiddleware")(le);

                    if (!isRequestLog)
                        return false;

                    // Excluir endpoints técnicos
                    if (le.Properties.TryGetValue("RequestPath", out var pathValue))
                    {
                        var path = pathValue.ToString();

                        return !path.Contains("/health",
                                   StringComparison.OrdinalIgnoreCase)
                            && !path.Contains("/swagger",
                                   StringComparison.OrdinalIgnoreCase);
                    }

                    return true;
                })

                .WriteTo.File(

                    // Archivo de auditoría
                    path: "logs/Notifications.API-audit-.log",

                    // JSON estructurado
                    formatter: new JsonFormatter(),

                    // Un archivo por día
                    rollingInterval: RollingInterval.Day,

                    // Mantener últimos 30 archivos
                    retainedFileCountLimit: 30,

                    // Compartido entre procesos
                    shared: true))

            .CreateLogger();

        builder.Host.UseSerilog();
    }
}