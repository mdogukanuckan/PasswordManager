using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Exceptions;

namespace PasswordManager.API.Middleware.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials", exception.Message),
            EmailAlreadyExistsException => (StatusCodes.Status409Conflict, "Email already exists", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "An unexpected error occurred. Please try again later.")
        };
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        }, cancellationToken);
        return true;
    }
}