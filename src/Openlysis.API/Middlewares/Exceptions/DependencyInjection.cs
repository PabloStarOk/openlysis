namespace Openlysis.API.Middlewares.Exceptions;

/// <summary>
/// Provides methods for registering exception handlers in the service collection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds exception handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the exception handlers will be added.</param>
    public static void AddExceptionHandlers(
        this IServiceCollection services)
    {
        // First specific exception handlers
        services.AddExceptionHandler<InvalidDataExceptionHandler>();

        // Global exception handler for 500 status codes.
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}