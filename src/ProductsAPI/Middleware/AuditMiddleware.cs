using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ProductsAPI.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;
        private const string ServicioName = "Products.API";

        // Operaciones para las cuales capturamos el Body (escribir el body de un GET suele estar vacío)
        private static readonly HashSet<string> AuditBodyMethods =
            new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE", "PATCH" };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Leer el Correlation ID que fue generado previamente por el CorrelationIdMiddleware
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

            // 2. Loggear Inicio del request (Requisito 5.3)
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
                // Ejecutar el resto de la cadena de middlewares y llegar al Controller
                await _next(context);

                // 4. Capturar resultados y duración (Requisito 5.3)
                sw.Stop();
                memStream.Position = 0;
                var responseBody = await new StreamReader(memStream).ReadToEndAsync();

                memStream.Position = 0;
                await memStream.CopyToAsync(originalResponseBody);

                var statusCode = context.Response.StatusCode;

                // Extraer el errorCode si la respuesta es un JSON estructurado de error
                var responseJsonDict = TryParseAndSanitizeJson(responseBody, context.Request.Path);
                string? errorCode = null;

                if (responseJsonDict != null && responseJsonDict.TryGetValue("errorCode", out var codeObj))
                {
                    errorCode = codeObj?.ToString();
                }

                // 5. Loggear Fin del request con niveles dinámicos según exige el TP
                if (statusCode >= 500)
                {
                    // Errores inesperados como Error
                    _logger.LogError(
                        "FIN Request (ERROR) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | ErrorCode: {ErrorCode} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, errorCode, requestBody, responseJsonDict ?? (object)responseBody);
                }
                else if (statusCode >= 400)
                {
                    // Errores de negocio/validación como Warning
                    _logger.LogWarning(
                        "FIN Request (WARNING) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | ErrorCode: {ErrorCode} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, errorCode, requestBody, responseJsonDict ?? (object)responseBody);
                }
                else
                {
                    // Éxito como Information
                    _logger.LogInformation(
                        "FIN Request (OK) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, requestBody, responseJsonDict ?? (object)responseBody);
                }
            }
            finally
            {
                // Restaurar el stream original en caso de error crítico
                context.Response.Body = originalResponseBody;
            }
        }

        private static async Task<string> ReadBodyAsync(Stream body)
        {
            using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private static Dictionary<string, object>? TryParseAndSanitizeJson(string raw, PathString path)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            try
            {
                // En Products.API no hay datos sensibles en los requests/responses 
                // (como tarjetas de crédito o contraseñas), por lo que podemos 
                // serializar el JSON directamente para que Serilog lo formatee lindo.
                return JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
            }
            catch
            {
                return null;
            }
        }
    }
}