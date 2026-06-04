using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Users.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Intentar obtener el ID del header 
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) || string.IsNullOrWhiteSpace(correlationId))
        {
            // 2. Si no existe, generar uno nuevo
            correlationId = Guid.NewGuid().ToString();
        }

        
        context.TraceIdentifier = correlationId.ToString();

        
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, context.TraceIdentifier);
            }
            return Task.CompletedTask;
        });

        
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            await _next(context);
        }
    }
}