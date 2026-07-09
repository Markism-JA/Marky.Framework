using Marky.Framework.Domain;
using Marky.Framework.Persistence.Abstraction;
using Marky.Framework.Persistence.EntityFramework.Interceptor;
using Marky.Framework.Persistence.EntityFramework.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Marky.Framework.Persistence.EntityFramework;

public static class DependencyInjection
{
    public static IServiceCollection AddFrameworkPersistenceEngine<TResolver>(
        this IServiceCollection services
    )
        where TResolver : class, IEntityContextResolver
    {
        services.AddScoped<PersistenceAuditInterceptor>();
        services.AddScoped<OutboxRedirectInterceptor>();

        services.AddSingleton<IEntityContextResolver, TResolver>();

        services.AddScoped(typeof(Repository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(DynamicRepositoryProxy<>));

        return services;
    }
}
