using MessagePack;
using MessagePack.Resolvers;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Openlysis.API.Serialization;
using Openlysis.Application.Common.Abstractions.Services;
using Openlysis.Domain.Files;
using Openlysis.Domain.Files.Entities;
using Openlysis.Domain.URLs;
using Openlysis.Domain.URLs.Entities;

namespace Openlysis.API.Notifications;

/// <summary>
/// Provides extension methods for registering push notification services.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Adds push notification related services to the specified <see cref="IServiceCollection"/>.
    /// Registers SignalR with MessagePack protocol, configures JWT options, and adds notifiers.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    public static void AddPushNotifications(this IServiceCollection services)
    {
        services.AddSignalR().AddMessagePackProtocol(options =>
        {
            options.SerializerOptions = MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    [new DateTimeUnixEpochFormatter()],
                    [ContractlessStandardResolver.Instance]))
                .WithSecurity(MessagePackSecurity.UntrustedData);
        });
        ConfigureJwtOptions(services);
        AddNotifiers(services);
    }

    private static void ConfigureJwtOptions(IServiceCollection services)
    {
        services.Configure<JwtBearerOptions>(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query[NotificationConstants.AccessTokenQuery];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken)
                        && path.StartsWithSegments(NotificationConstants.HubsRootPath))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
            };
        });
    }

    private static void AddNotifiers(IServiceCollection services)
    {
        services.AddScoped<IMessageAnalysisUpdatesNotifier, MessageAnalysisUpdatesNotifier>();
        services.AddScoped<
            IMultiAnalysisUpdatesNotifier<FileMultiAnalysis, FileAnalysis>,
            MultiAnalysisUpdatesNotifier<FileMultiAnalysis, FileAnalysis>>();
        services.AddScoped<
            IMultiAnalysisUpdatesNotifier<UrlMultiAnalysis, UrlAnalysis>,
            MultiAnalysisUpdatesNotifier<UrlMultiAnalysis, UrlAnalysis>>();
    }
}