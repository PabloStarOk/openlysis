using Microsoft.AspNetCore.Diagnostics;

namespace Openlysis.API.Middlewares.Exceptions;

/// <summary>
/// Handles exceptions of type <see cref="InvalidDataException"/> and generates a validation problem response.
/// </summary>
public class InvalidDataExceptionHandler : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidDataException invalidDataException)
        {
            return false;
        }

        IResult result = Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            detail: invalidDataException.Message);

        await result.ExecuteAsync(httpContext);
        return true;
    }
}