using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Marky.Toolkit.Migration.Interceptors;

/// <summary>
/// Captures and alerts on slow-running queries during design-time or development executions.
/// </summary>
public class PerformanceLoggingInterceptor(
    ILogger<PerformanceLoggingInterceptor> logger,
    long thresholdMs = 100
) : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result
    )
    {
        EvaluateDuration(command, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default
    )
    {
        EvaluateDuration(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void EvaluateDuration(DbCommand command, TimeSpan duration)
    {
        if (duration.TotalMilliseconds > thresholdMs)
        {
            logger.LogWarning(
                "[PERF ALERT] Slow Query Detected ({Duration}ms): {Sql} | Parameters: {Params}",
                duration.TotalMilliseconds,
                command.CommandText,
                string.Join(
                    ", ",
                    command
                        .Parameters.Cast<DbParameter>()
                        .Select(p => $"{p.ParameterName}={p.Value}")
                )
            );
        }
    }
}
