using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Marky.Framework.Web;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(
            exception,
            "A critical system kernel execution crash occurred during an active HTTP request context: {Message}",
            exception.Message
        );

        var errorResponse = new ErrorResponse
        {
            Code = "InternalServerError",
            Description = "An unexpected fatal runtime error occurred within the system kernel.",
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            errorResponse,
            cancellationToken: cancellationToken
        );

        return true;
    }
}
