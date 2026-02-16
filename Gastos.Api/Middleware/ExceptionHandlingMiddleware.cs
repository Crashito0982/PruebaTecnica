using Microsoft.AspNetCore.Mvc;

namespace Gastos.Api.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            var pd = new ProblemDetails
            {
                Title = ex.Title,
                Detail = ex.Message,
                Status = ex.StatusCode,
                Type = $"https://httpstatuses.com/{ex.StatusCode}"
            };

            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(pd);
        }
    }
}

public abstract class ApiException : Exception
{
    protected ApiException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
    public int StatusCode { get; }
    public string Title { get; }
}

public sealed class NotFoundApiException : ApiException
{
    public NotFoundApiException(string message) : base(404, "Not Found", message) { }
}

public sealed class ForbiddenApiException : ApiException
{
    public ForbiddenApiException(string message) : base(403, "Forbidden", message) { }
}

public sealed class ConflictApiException : ApiException
{
    public ConflictApiException(string message) : base(409, "Conflict", message) { }
}

public sealed class BadRequestApiException : ApiException
{
    public BadRequestApiException(string message) : base(400, "Bad Request", message) { }
}
