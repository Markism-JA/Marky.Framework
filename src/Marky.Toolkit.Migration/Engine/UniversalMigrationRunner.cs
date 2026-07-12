using System.Reflection;
using System.Text.RegularExpressions;
using Marky.Toolkit.Migration.Abstractions;
using Marky.Toolkit.Migration.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marky.Toolkit.Migration.Engine;

public static class UniversalMigrationRunner
{
    public static async Task RunAsync(string[] args, Assembly[]? assembliesToScan)
    {
        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger("UniversalMigrationRunner");

        try
        {
            if (assembliesToScan == null || assembliesToScan.Length == 0)
            {
                logger.LogCritical(
                    "Assembly scan targets are null or empty. Host execution execution halted."
                );
                return;
            }

            var environment =
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"Environments/appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    configBuilder.AddUserSecrets(entryAssembly, optional: true);
                }
            }

            var config = configBuilder.Build();

            var targetKey = config["target"]?.ToUpperInvariant();
            if (string.IsNullOrEmpty(targetKey))
            {
                logger.LogError(
                    "Run parameter execution key missing. Provide command token: --target=GameServer or --target=All"
                );
                return;
            }

            var discoveredTargets = assembliesToScan
                .Distinct()
                .SelectMany(s =>
                {
                    try
                    {
                        return s.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        logger.LogWarning(
                            "Partial type configuration extraction hit on assembly '{Assembly}'. Using available fallback types.",
                            s.FullName
                        );
                        return ex.Types.Where(t => t != null)!;
                    }
                })
                .Where(p =>
                    typeof(IDbTargetDescriptor).IsAssignableFrom(p)
                    && !p.IsInterface
                    && !p.IsAbstract
                )
                .Select(t =>
                {
                    try
                    {
                        return (IDbTargetDescriptor)Activator.CreateInstance(t)!;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to instantiate data target descriptor plugin: '{Type}' via reflection allocation.",
                            t.FullName
                        );
                        return null;
                    }
                })
                .Where(x => x != null)
                .Cast<IDbTargetDescriptor>()
                .ToList();

            List<IDbTargetDescriptor> executionQueue;

            if (targetKey == "ALL")
            {
                executionQueue = discoveredTargets;
            }
            else
            {
                var targetKeys = targetKey
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                executionQueue = discoveredTargets
                    .Where(t => targetKeys.Contains(t.TargetKey))
                    .ToList();
            }

            if (!executionQueue.Any())
            {
                logger.LogWarning(
                    "No functional data storage descriptors found matching target key token: '{TargetKey}'",
                    targetKey
                );
                return;
            }

            var checker = new PreFlightConnectionChecker(
                loggerFactory.CreateLogger<PreFlightConnectionChecker>()
            );

            foreach (var target in executionQueue)
            {
                string connectionStringName = target.TargetKey;
                var connectionString = config.GetConnectionString(connectionStringName);

                if (string.IsNullOrEmpty(connectionString))
                {
                    logger.LogCritical(
                        "A valid connection string named '{Name}' could not be resolved for target context node: '{TargetKey}'.",
                        connectionStringName,
                        target.TargetKey
                    );
                    return;
                }

                var hostMatch = Regex.Match(
                    connectionString,
                    @"(?:Host|Server)\s*=\s*([^;]+)",
                    RegexOptions.IgnoreCase
                );
                var portMatch = Regex.Match(
                    connectionString,
                    @"Port\s*=\s*([0-9]+)",
                    RegexOptions.IgnoreCase
                );

                string dbHost = hostMatch.Success ? hostMatch.Groups[1].Value.Trim() : "localhost";
                int dbPort =
                    portMatch.Success && int.TryParse(portMatch.Groups[1].Value, out var parsedPort)
                        ? parsedPort
                        : 5432;

                logger.LogInformation(
                    "Invoking provider-agnostic pre-flight connection check on node [{TargetKey}] targeting {Host}:{Port}...",
                    target.TargetKey,
                    dbHost,
                    dbPort
                );
                if (!await checker.IsDbBoundaryLiveAsync(dbHost, dbPort, maxRetries: 3))
                {
                    logger.LogCritical(
                        "Network path to database target boundary context node [{TargetKey}] remains unreachable. Aborting deployment.",
                        target.TargetKey
                    );
                    return;
                }
            }

            foreach (var target in executionQueue)
            {
                logger.LogInformation(
                    "Executing pipeline target database configuration engine node: [{TargetKey}]",
                    target.TargetKey
                );

                try
                {
                    var services = new ServiceCollection();
                    services.AddSingleton(loggerFactory);
                    services.AddLogging(b => b.AddConsole());

                    target.ConfigureServices(services, config);

                    var provider = services.BuildServiceProvider();
                    using var scope = provider.CreateScope();

                    var orchestratorType = typeof(IMigrationOrchestrator<>).MakeGenericType(
                        target.DbContextType
                    );
                    var orchestrator = scope.ServiceProvider.GetRequiredService(orchestratorType);

                    var method = orchestratorType.GetMethod(
                        nameof(
                            IMigrationOrchestrator<Microsoft.EntityFrameworkCore.DbContext>.OrchestrateAsync
                        )
                    );
                    if (method == null)
                    {
                        logger.LogError(
                            "Critical Method Interception Fail: Execution tracking endpoint 'OrchestrateAsync' not found on resolved target runner."
                        );
                        continue;
                    }

                    await (Task)
                        method.Invoke(orchestrator, new object[] { default(CancellationToken) })!;
                    logger.LogInformation(
                        "Successfully completed schema iteration pipeline operations for target node [{TargetKey}].",
                        target.TargetKey
                    );
                }
                catch (Exception ex)
                {
                    logger.LogCritical(
                        ex,
                        "Uncaught process failure encountered during dynamic execution loop of database target boundary configuration: [{TargetKey}]",
                        target.TargetKey
                    );
                    throw;
                }
            }
        }
        catch (Exception rootEx)
        {
            logger.LogCritical(
                rootEx,
                "Fatal uncaught crash scenario encountered inside universal migration engine boundary framework execution layer."
            );
            throw;
        }
        finally
        {
            loggerFactory.Dispose();
        }
    }
}
