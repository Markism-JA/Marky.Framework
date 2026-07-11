using System.Reflection;
using FluentValidation;
using Marky.Framework.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Marky.Framework.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEnterpriseBaseDependency<TMarker>(
        this IServiceCollection services
    )
    {
        return services.AddEnterpriseBaseDependency(typeof(TMarker).Assembly);
    }

    public static IServiceCollection AddEnterpriseBaseDependency(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}
