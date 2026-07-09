using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Marky.Framework.Persistence.EntityFramework.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Scans the assembly and natively applies only the configurations that match the marker interface constraint.
    /// </summary>
    public static ModelBuilder ApplyCuratedConfigurations<TMarker>(
        this ModelBuilder modelBuilder,
        Assembly assembly
    )
    {
        return modelBuilder.ApplyConfigurationsFromAssembly(
            assembly,
            type => typeof(TMarker).IsAssignableFrom(type)
        );
    }
}
