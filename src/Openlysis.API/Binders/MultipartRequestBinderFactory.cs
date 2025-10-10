using FastEndpoints;

namespace Openlysis.API.Binders;

/// <summary>
/// Factory for binding multipart requests to <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The request type to bind.</typeparam>
internal sealed class MultipartRequestBinderFactory<TRequest> : IRequestBinder<TRequest>
    where TRequest : class
{
    /// <inheritdoc/>
    public async ValueTask<TRequest> BindAsync(BinderContext ctx, CancellationToken ct)
    {
        var binder = ctx.HttpContext.RequestServices.GetRequiredService<MultipartRequestBinder<TRequest>>();
        return await binder.BindAsync(ctx, ct);
    }
}