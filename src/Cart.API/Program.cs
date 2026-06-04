using Cart.API.Data;
using CartAPI.Data;
using CartAPI.Services;
using CartPI.Data;
using Dapper;


var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────────────────
// SERVICES
// ──────────────────────────────────────────────────────────────────────────

builder.Services.AddScoped<CartRepository>();

builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddHttpClient(
    "ProductsAPI",
    client =>
    {
        client.BaseAddress =
            new Uri("http://localhost:7001");
    });

builder.Services.AddSingleton<DatabaseInitializer>();

SqlMapper.AddTypeHandler(new GuidTypeHandler());

var app = builder.Build(); // Hay que borrarlo antes del merge porque despues lo vamos a integrar todo


// ──────────────────────────────────────────────────────────────────────────
// DATABASE INITIALIZATION
// ──────────────────────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var initializer =
        scope.ServiceProvider
            .GetRequiredService<DatabaseInitializer>();

    initializer.Initialize();
}