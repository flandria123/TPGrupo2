using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Orders.API.Middleware 
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        // Solo auditar operaciones de escritura (Crear orden, Actualizar estado)
        private static readonly HashSet<string> AuditMethods =
            new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE" };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si no es una operación de escritura (ej. GET), pasamos directo para no sobrecargar el log
            if (!AuditMethods.Contains(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // ── Capturar Request body ──────────────────────────────────────────
            context.Request.EnableBuffering(); // permite releer el stream
            var requestBody = await ReadBodyAsync(context.Request.Body);
            context.Request.Body.Position = 0; // rebobinar para que el controlador lo pueda leer

            // ── Capturar Response body ─────────────────────────────────────────
            var originalResponseBody = context.Response.Body;
            var memStream = new MemoryStream();
            context.Response.Body = memStream;

            string responseBody = string.Empty;

            try
            {
                await _next(context); // ejecutar el pipeline y los controladores

                // Si todo sale bien (código 200/201/204), leemos y copiamos la respuesta
                memStream.Position = 0;
                responseBody = await new StreamReader(memStream).ReadToEndAsync();

                memStream.Position = 0;
                await memStream.CopyToAsync(originalResponseBody);
            }
            finally
            {
                // ¡LÍNEA SALVAVIDAS!
                // Se ejecuta SIEMPRE. Si el Controller lanza una ValidationException, 
                // la ejecución salta directo aquí. Le devolvemos a .NET su stream original
                // para que el ValidationExceptionHandler pueda dibujar el error 400.
                context.Response.Body = originalResponseBody;

                // Destruimos la memoria temporal de forma segura
                memStream.Dispose();
            }

            // ── Escribir entrada de auditoría ──────────────────────────────────
            _logger.LogInformation(
                "AUDIT {@Method} {@Path} {@StatusCode} {@RequestBody} {@ResponseBody}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                TryParseJson(requestBody),
                TryParseJson(responseBody));
        }


        private static async Task<string> ReadBodyAsync(Stream body)
        {
            using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        // Deserializar para que Serilog lo guarde como objeto JSON anidado (no como un string escapado)
        private static object? TryParseJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            try
            {
                // A diferencia de Users API, acá no ofuscamos passwords porque Orders no maneja esos datos.
                // Si en el futuro el TP les pide procesar tarjetas de crédito, se enmascararían en este bloque.
                return JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
            }
            catch
            {
                return raw; // Si no es un JSON válido, lo devuelve como texto plano
            }
        }
    }
}