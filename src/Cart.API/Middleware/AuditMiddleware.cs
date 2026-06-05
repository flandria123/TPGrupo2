using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CartAPI.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;
        private const string ServicioName = "Cart.API";

        // Operaciones para las cuales capturamos el Body
        private static readonly HashSet<string> AuditBodyMethods =
            new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE", "PATCH" };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Leer el Correlation ID generado por el CorrelationIdMiddleware
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

            // 2. Loggear Inicio del request (Requisito 5.3 del TP)
            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "INICIO Request | Servicio: {Servicio} | Endpoint: {Method} {Path} | CorrelationId: {CorrelationId}",
                ServicioName, context.Request.Method, context.Request.Path.Value, correlationId);

            // 3. Capturar Body del Request (si aplica)
            string? requestBody = null;
            if (AuditBodyMethods.Contains(context.Request.Method))
            {
                context.Request.EnableBuffering();
                requestBody = await ReadBodyAsync(context.Request.Body);
                context.Request.Body.Position = 0;
            }

            // Preparar captura del Response Body
            var originalResponseBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                // Ejecutar el resto de la API
                await _next(context);

                // 4. Capturar resultados y duración
                sw.Stop();
                memStream.Position = 0;
                var responseBody = await new StreamReader(memStream).ReadToEndAsync();

                memStream.Position = 0;
                await memStream.CopyToAsync(originalResponseBody);

                var statusCode = context.Response.StatusCode;

                // Extraer el errorCode si la respuesta es un JSON estructurado
                var responseJsonDict = TryParseJson(responseBody);
                string? errorCode = null;

                if (responseJsonDict != null && responseJsonDict.TryGetValue("errorCode", out var codeObj))
                {
                    errorCode = codeObj?.ToString();
                }

                // 5. Loggear Fin del request con niveles dinámicos
                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "FIN Request (ERROR) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | ErrorCode: {ErrorCode} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, errorCode, requestBody, responseJsonDict ?? (object)responseBody);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "FIN Request (WARNING) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | ErrorCode: {ErrorCode} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, errorCode, requestBody, responseJsonDict ?? (object)responseBody);
                }
                else
                {
                    _logger.LogInformation(
                        "FIN Request (OK) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, requestBody, responseJsonDict ?? (object)responseBody);
                }
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }

        private static async Task<string> ReadBodyAsync(Stream body)
        {
            using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private static Dictionary<string, object>? TryParseJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            try
            {
                // En Cart API serializamos directo, sin lógica de ofuscación de contraseñas
                return JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
            }
            catch
            {
                return null;
            }
        }
    }
}