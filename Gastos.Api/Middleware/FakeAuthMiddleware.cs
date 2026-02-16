using Gastos.Api.Auth;
using Gastos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gastos.Api.Middleware;

public sealed class FakeAuthMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue("X-User-Id", out var header) ||
            !Guid.TryParse(header.ToString(), out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                status = 401,
                detail = "Falta header X-User-Id válido."
            });
            return;
        }

        var db = context.RequestServices.GetRequiredService<GastosDbContext>();

        // Nota: si luego implementamos soft-delete/bloqueo, lo validamos aquí.
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                status = 401,
                detail = "Usuario no existe."
            });
            return;
        }

        if (user.IsBlocked)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Forbidden",
                status = 403,
                detail = "Usuario bloqueado no puede operar."
            });
            return;
        }

        var currentUser = context.RequestServices.GetRequiredService<CurrentUser>();
        currentUser.Set(userId);

        await next(context);
    }
}
