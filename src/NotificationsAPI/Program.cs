using NotificationsAPI.ExceptionHandlers;
using NotificationsAPI.Extensions;

public partial class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
                                         
        // ─────────────────────────────────────────────
        // LOGGING (SERILOG)
        // ─────────────────────────────────────────────
        builder.AddAppLogging();

        // ─────────────────────────────────────────────
        // SERVICES
        // ─────────────────────────────────────────────
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // ─────────────────────────────────────────────
        // EXCEPTION HANDLERS
        // ─────────────────────────────────────────────
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // ─────────────────────────────────────────────
        // SWAGGER
        // ─────────────────────────────────────────────
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // ─────────────────────────────────────────────
        // EXCEPTION PIPELINE
        // ─────────────────────────────────────────────
        app.UseExceptionHandler();

        // ─────────────────────────────────────────────
        // CUSTOM MIDDLEWARE
        // ─────────────────────────────────────────────
        app.UseAppMiddleware();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    

}