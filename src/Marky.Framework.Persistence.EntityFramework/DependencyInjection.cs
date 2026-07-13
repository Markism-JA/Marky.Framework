using Marky.Framework.Domain;
using Marky.Framework.Persistence.Abstraction;
using Marky.Framework.Persistence.EntityFramework.Interceptor;
using Marky.Framework.Persistence.EntityFramework.Outbox;
using Marky.Framework.Persistence.EntityFramework.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Marky.Framework.Persistence.EntityFramework;

public static class DependencyInjection
{
    public static IServiceCollection AddFrameworkPersistenceEngine<TResolver>(
        this IServiceCollection services
    )
        where TResolver : class, IEntityContextResolver
    {
        services.TryAddScoped<OutboxRedirectState>();
        services.AddScoped<PersistenceAuditInterceptor>();
        services.AddScoped<OutboxRedirectInterceptor>();

        services.AddSingleton<IEntityContextResolver, TResolver>();

        services.AddScoped(typeof(Repository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(DynamicRepositoryProxy<>));

        return services;
    }

    public static IServiceCollection AddCoordinatedContext<TContext>(
        this IServiceCollection services,
        string clusterKey,
        Action<DbContextOptionsBuilder> optionsAction
    )
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(
            (sp, optionsBuilder) =>
            {
                optionsAction(optionsBuilder);
                var environment = sp.GetRequiredService<IHostEnvironment>();

                if (environment.IsDevelopment())
                {
                    optionsBuilder.EnableSensitiveDataLogging();
                    optionsBuilder.EnableDetailedErrors();
                }
                optionsBuilder.AddInterceptors(
                    sp.GetRequiredService<PersistenceAuditInterceptor>(),
                    sp.GetRequiredService<OutboxRedirectInterceptor>()
                );
            }
        );

        services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.AddKeyedScoped<DbContext, TContext>(
            clusterKey,
            (sp, _) => sp.GetRequiredService<TContext>()
        );

        return services;
    }
}
