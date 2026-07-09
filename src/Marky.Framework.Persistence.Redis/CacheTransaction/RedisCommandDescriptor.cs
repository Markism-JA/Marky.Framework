using StackExchange.Redis;

namespace Marky.Framework.Persistence.Redis.CacheTransaction;

/// <summary>
/// A flat memory wrapper that binds a stateless execution callback pointer
/// together with its raw data state variables to prevent heap closure allocations.
/// </summary>
public readonly record struct RedisCommandDescriptor(
    Func<IBatch, string, string, string?, Task> Callback,
    string Key,
    string Value,
    string? ExtraState = null
);
