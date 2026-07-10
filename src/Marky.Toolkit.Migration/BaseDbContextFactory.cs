using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Marky.Toolkit.Migration;

public abstract class BaseDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    protected abstract string ConnectionStringKey { get; }

    public TContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration(args);
        var connectionString = configuration.GetConnectionString(ConnectionStringKey);
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        ConfigureProvider(optionsBuilder, connectionString, configuration);

        return CreateContextInstance(optionsBuilder.Options);
    }

    protected abstract void ConfigureProvider(
        DbContextOptionsBuilder<TContext> optionsBuilder,
        string? connectionString,
        IConfiguration configuration
    );

    protected abstract TContext CreateContextInstance(DbContextOptions<TContext> options);
    protected abstract IConfiguration BuildConfiguration(string[] args);
}
