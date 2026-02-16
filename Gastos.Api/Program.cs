using Gastos.Api.Auth;
using Gastos.Api.Middleware;
using Gastos.Application.Abstractions;
using Gastos.Domain.Entities;
using Gastos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gastos API",
        Version = "v1"
    });

    options.AddSecurityDefinition("X-User-Id", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-User-Id",
        Description = """
Se puede usar el header X-User-Id (fake auth). Pegá uno de estos GUID:

- 11111111-1111-1111-1111-111111111111 (Usuario Demo 1)
- 22222222-2222-2222-2222-222222222222 (Usuario Demo 2)
"""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-User-Id"
                }
            },
            Array.Empty<string>()
        }
    });
});


// DbContext
builder.Services.AddDbContext<GastosDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Current user + middleware
builder.Services.AddScoped<ExceptionHandlingMiddleware>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
builder.Services.AddScoped<FakeAuthMiddleware>();

var app = builder.Build();

// Swagger solo en dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Auth fake (X-User-Id)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<FakeAuthMiddleware>();

// Endpoint simple para comprobar que responde
app.MapGet("/ping", () => Results.Ok("API Gastos OK"));

// Seed + auto-migrate (práctico para dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GastosDbContext>();

    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Nombre="Usuario Demo 1", Email="demo1@example.com", FechaCreacion=DateTime.UtcNow },
            new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Nombre="Usuario Demo 2", Email="demo2@example.com", FechaCreacion=DateTime.UtcNow }
        );
        db.SaveChanges();
    }

    if (app.Environment.IsDevelopment())
    {
        DevSeeder.Seed(db);
    }
}

// Controllers endpoints
app.MapControllers();

app.Run();
