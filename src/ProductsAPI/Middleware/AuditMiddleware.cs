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
            // 1. Generar o propagar el X-Correlation-Id (Requisito 5.5)
            if (!context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationIdValues))
            {
                correlationIdValues = Guid.NewGuid().ToString();
                context.Request.Headers["X-Correlation-Id"] = correlationIdValues;
            }
            var correlationId = correlationIdValues.ToString();
            context.Items["CorrelationId"] = correlationId;

            // Inyectarlo en los headers de la respuesta para el cliente
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Correlation-Id"] = correlationId;
                return Task.CompletedTask;
            });

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
                // Ejecutar el resto de la API
                await _next(context);

                // 4. Capturar resultados y duración (Requisito 5.3)
                sw.Stop();
                memStream.Position = 0;
                var responseBody = await new StreamReader(memStream).ReadToEndAsync();

                memStream.Position = 0;
                await memStream.CopyToAsync(originalResponseBody);

                var statusCode = context.Response.StatusCode;

                // Extraer el errorCode si es un JSON de error (Requisito 5.3)
                var responseJsonDict = TryParseAndSanitizeJson(responseBody, context.Request.Path);
                string? errorCode = null;

                if (responseJsonDict != null && responseJsonDict.TryGetValue("errorCode", out var codeObj))
                {
                    errorCode = codeObj?.ToString();
                }

                // 5. Loggear Fin del request con niveles dinámicos (Requisito 5.3)
                if (statusCode >= 500)
                {
                    // Errores inesperados como Error
                    _logger.LogError(
                        "FIN Request (ERROR) | Servicio: {Servicio} | Endpoint: {Method} {Path} | Status: {StatusCode} | Duracion: {Duracion}ms | CorrelationId: {CorrelationId} | ErrorCode: {ErrorCode} | Request: {@RequestBody} | Response: {@ResponseBody}",
                        ServicioName, context.Request.Method, context.Request.Path.Value, statusCode, sw.ElapsedMilliseconds, correlationId, errorCode, requestBody, responseJsonDict ?? (object)responseBody);
                }
                else if (statusCode >= 400)
                {
                    // Errores de negocio como Warning
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
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
                if (jsonDict == null) return null;

                if (path.Value?.Contains("/users") == true)
                {
                    if (jsonDict.ContainsKey("password")) jsonDict["password"] = "********";
                    if (jsonDict.ContainsKey("Password")) jsonDict["Password"] = "********";
                }

                return jsonDict;
            }
            catch
            {
                return null;
            }
        }
    }
}