using Marky.Toolkit.Migration.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marky.Toolkit.Migration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Bundles the migration engine components cleanly into the host application dependency graph.
    /// </summary>
    public static IServiceCollection AddMarkyMigrationEngine<TContext>(
        this IServiceCollection services
    )
        where TContext : DbContext
    {
        services.AddScoped<IMigrationOrchestrator<TContext>, MigrationOrchestrator<TContext>>();
        return services;
    }
}
