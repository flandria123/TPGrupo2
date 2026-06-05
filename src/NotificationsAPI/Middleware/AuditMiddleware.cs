using System.Text;
using System.Text.Json;

namespace NotificationsAPI.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    // Solo auditar operaciones de escritura
    private static readonly HashSet<string> AuditMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "POST",
            "PUT",
            "DELETE"
        };

    public AuditMiddleware(
        RequestDelegate next,
        ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ignorar endpoints técnicos
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Solo auditar escrituras
        if (!AuditMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // ─────────────────────────────────────────────
        // REQUEST BODY
        // ─────────────────────────────────────────────
        context.Request.EnableBuffering();

        var requestBody = await ReadBodyAsync(context.Request.Body);

        // Rebobinar stream
        context.Request.Body.Position = 0;

        // ─────────────────────────────────────────────
        // RESPONSE BODY
        // ─────────────────────────────────────────────
        var originalResponseBody = context.Response.Body;

        using var memoryStream = new MemoryStream();

        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Position = 0;

        var responseBody =
            await new StreamReader(memoryStream).ReadToEndAsync();

        // Restaurar response original
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalResponseBody);

        context.Response.Body = originalResponseBody;

        // ─────────────────────────────────────────────
        // AUDIT LOG
        // ─────────────────────────────────────────────
        var correlationId =
            context.Items["CorrelationId"]?.ToString();

        _logger.LogInformation(
            "AUDIT {@CorrelationId} {@Method} {@Path} {@StatusCode} {@RequestBody} {@ResponseBody}",
            correlationId,
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            TryParseJson(requestBody),
            TryParseJson(responseBody));
    }

    private static async Task<string> ReadBodyAsync(Stream body)
    {
        using var reader =
            new StreamReader(body, Encoding.UTF8, leaveOpen: true);

        return await reader.ReadToEndAsync();
    }

    
    private static object? TryParseJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
        }
        catch
        {
            
            return raw;
        }
    }
}

