using System.Linq.Expressions;
using Marky.Framework.Domain;
using Microsoft.EntityFrameworkCore;

namespace Marky.Framework.Persistence.EntityFramework.Extensions;

public static class DbContextExtensions
{
    /// <summary>
    /// Dynamically applies a 'IsDeleted == false' filter to all entities implementing <see cref="ISoftDelete"/>.
    /// </summary>
    public static void ApplySoftDeleteFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var comparison = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(comparison, parameter);
    }
}
