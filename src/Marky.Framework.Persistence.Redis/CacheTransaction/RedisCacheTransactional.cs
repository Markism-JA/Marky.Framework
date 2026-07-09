using System.Collections.Concurrent;
using Marky.Framework.Persistence.Abstraction;
using StackExchange.Redis;

namespace Marky.Framework.Persistence.Redis.CacheTransaction;

public abstract class RedisCacheTransactional(IConnectionMultiplexer connection, int databaseId)
    : ICacheTransactional
{
    protected readonly IDatabase Database = connection.GetDatabase(databaseId);
    private readonly ConcurrentQueue<RedisCommandDescriptor> _commandQueue = new();
    private const int _chunkSize = 200;

    /// <summary>
    /// Safely pushes an execution routine onto the transactional memory buffer.
    /// </summary>
    protected void EnqueueOperation(
        Func<IBatch, string, string, string?, Task> callback,
        string key,
        string value,
        string? extraState = null
    )
    {
        _commandQueue.Enqueue(
            new RedisCommandDescriptor(
                Callback: callback,
                Key: key,
                Value: value,
                ExtraState: extraState
            )
        );
    }

    /// <inheritdoc />
    public async Task FlushChangesAsync()
    {
        while (!_commandQueue.IsEmpty)
        {
            await FlushChunkPartitionAsync();
        }
    }

    private async Task FlushChunkPartitionAsync()
    {
        var batchCommands = new List<RedisCommandDescriptor>(_chunkSize);

        while (batchCommands.Count < _chunkSize && _commandQueue.TryDequeue(out var command))
        {
            batchCommands.Add(command);
        }

        if (batchCommands.Count == 0)
            return;

        try
        {
            var batch = Database.CreateBatch();

            // Dynamic Execution: Trigger the delegate pointer natively
            foreach (var cmd in batchCommands)
            {
                cmd.Callback(batch, cmd.Key, cmd.Value, cmd.ExtraState);
            }

            batch.Execute();
            await Task.Yield();
        }
        catch (Exception)
        {
            ClearBufferedChanges();
            throw;
        }
    }

    /// <inheritdoc />
    public void ClearBufferedChanges()
    {
        while (_commandQueue.TryDequeue(out _)) { }
    }
}
