using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Marky.Toolkit.Migration.Diagnostics;

/// <summary>
/// Prevents boot crashes by ensuring network socket lines to database boundaries are live.
/// </summary>
public class PreFlightConnectionChecker(ILogger<PreFlightConnectionChecker> logger)
{
    public async Task<bool> IsDbBoundaryLiveAsync(
        string host,
        int port,
        int maxRetries = 5,
        int delayMs = 2000
    )
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var client = new TcpClient();
                // Avoid hanging indefinitely with a swift 2-second timeout
                var connectTask = client.ConnectAsync(host, port);
                var delayTask = Task.Delay(2000);

                if (await Task.WhenAny(connectTask, delayTask) == connectTask)
                {
                    await connectTask; // Flush exceptions
                    logger.LogInformation(
                        "Successfully verified connectivity to database boundary {Host}:{Port}.",
                        host,
                        port
                    );
                    return true;
                }
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Database boundary {Host}:{Port} not ready yet. Retry {Current} of {Max}.",
                    host,
                    port,
                    i + 1,
                    maxRetries
                );
            }

            await Task.Delay(delayMs);
        }

        return false;
    }
}
