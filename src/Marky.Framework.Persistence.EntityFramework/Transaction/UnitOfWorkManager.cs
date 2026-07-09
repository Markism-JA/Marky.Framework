using Marky.Framework.Persistence.Abstraction;
using Marky.Framework.Persistence.EntityFramework.Interceptor;
using Marky.Framework.Persistence.EntityFramework.Marker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Marky.Framework.Persistence.EntityFramework.Transaction;

public class UnitOfWorkManager(IServiceProvider serviceProvider, string clusterKey)
    : IUnitOfWorkManager
{
    public async Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken ct = default)
    {
        var registeredContexts = serviceProvider.GetKeyedServices<DbContext>(clusterKey).ToList();
        var cacheTransactions = serviceProvider
            .GetKeyedServices<ICacheTransactional>(clusterKey)
            .ToList();

        var primaryOutboxContext = registeredContexts.FirstOrDefault(c =>
            c is IOutboxContextMarker
        );
        OutboxRedirectInterceptor? activeInterceptor = null;

        if (primaryOutboxContext is not null)
        {
            activeInterceptor = serviceProvider.GetRequiredService<OutboxRedirectInterceptor>();
            activeInterceptor.RedirectTo(primaryOutboxContext);
        }

        var activeTransactions = new List<IDbContextTransaction>();

        try
        {
            var connectionGroups = registeredContexts
                .GroupBy(c => c.Database.GetConnectionString())
                .ToList();
            foreach (var group in connectionGroups)
            {
                var contextList = group.ToList();
                var primaryContext = contextList.First();

                var transaction = await primaryContext.Database.BeginTransactionAsync(ct);
                activeTransactions.Add(transaction);

                var dbTransaction = transaction.GetDbTransaction();
                foreach (var secondaryContext in contextList.Skip(1))
                {
                    await secondaryContext.Database.UseTransactionAsync(dbTransaction, ct);
                }
            }
        }
        catch (Exception)
        {
            foreach (var tx in activeTransactions)
                await tx.DisposeAsync();
            activeInterceptor?.ClearRedirect();
            throw;
        }

        return new UnitOfWorkScope(
            registeredContexts,
            activeTransactions,
            cacheTransactions,
            activeInterceptor
        );
    }
}
