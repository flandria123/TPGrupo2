using Serilog;
using Serilog.Events;

// se configura el middleware que intercepta cada request HTTP y genera el evento de log:

namespace Users.API.Extensions
{
    public static class MiddlewareExtensions
    {

        public static void UseAppMiddleware(this WebApplication app)
        {
            // Acá se puede agregar cualquier middleware personalizado que necesite                 
                      
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
               (ex != null) ? LogEventLevel.Error :
               (httpContext.Request.Path.StartsWithSegments("/health"))
               ? LogEventLevel.Verbose : LogEventLevel.Information;


            });


        }




    }
}
