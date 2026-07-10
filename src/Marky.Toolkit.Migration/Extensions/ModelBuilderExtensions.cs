using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Marky.Toolkit.Migration.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Forces standard snake_case schemas dynamically to prevent double-quote escape hell in SQL flavors like Postgres.
    /// </summary>
    public static void ApplySnakeCaseNamingConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
            }
        }
    }

    /// <summary>
    /// Automates the extraction and attachment of all mapping metadata layers in a target assembly.
    /// </summary>
    public static void ApplyAllConfigurationsFromAssembly(
        this ModelBuilder modelBuilder,
        Assembly assembly
    )
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }

    private static string ToSnakeCase(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        return Regex.Replace(input, @"(?<!^)(?=[A-Z])", "_").ToLowerInvariant();
    }
}
