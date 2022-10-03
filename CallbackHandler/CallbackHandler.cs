using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CallbackHandler;

public class CallbackHandler<T> : ICallbackHandler
{
    private readonly IMessageBroadcaster _broadcaster;
    private readonly CallbackHandlerConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly MethodInfo? _cacheMethodInfo;
    private readonly PropertyInfo? _cacheMethodInfoResult;

    private ConcurrentDictionary<string, TaskCompletionSource<T>> _callbacks =
        new();

    private ConcurrentDictionary<string, T> _waitingResults = new();

    private ConcurrentQueue<(DateTime, string)> _waitingResultCache =
        new();

    public CallbackHandler(IMessageBroadcaster broadcaster, CallbackHandlerConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _broadcaster = broadcaster;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _broadcaster.Subscribe((string id, T message) => { InternalSetResult(id, message); });
        if (configuration.CacheHandler != null)
        {
            _cacheMethodInfo = configuration.CacheHandler.GetMethod("GetResult");
            _cacheMethodInfoResult = _cacheMethodInfo.ReturnType.GetProperty("Result");
        }
    }

    public async Task<T> WaitResult(string id, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
    {
        if (_waitingResults.TryGetValue(id, out var data))
        {
            return data;
        }

        if (_cacheMethodInfo != null)
        {
            var cacheHandler = _serviceProvider.GetRequiredService(_configuration.CacheHandler);
            var task = ((Task) _cacheMethodInfo.Invoke(cacheHandler, new object?[] {id}));
            await task.ConfigureAwait(false);
            return (T)_cacheMethodInfoResult.GetValue(task);
        }

        var tcs = new TaskCompletionSource<T>();
        _callbacks.TryAdd(id, tcs);

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.Register(() =>
            {
                _callbacks.TryRemove(id, out _);
                tcs.SetCanceled(cancellationToken);
            });
        }

        if (timeout != null)
        {
            using var cts = new CancellationTokenSource(timeout.Value);
            cts.Token.Register(() =>
            {
                _callbacks.TryRemove(id, out _);
                tcs.SetCanceled(cts.Token);
            });
            var result = await tcs.Task.ConfigureAwait(false);
            _callbacks.TryRemove(id, out _);
            return result;
        }
        else
        {
            var result = await tcs.Task;
            _callbacks.TryRemove(id, out _);
            return result;
        }
    }

    public async Task SetResult(string id, T data)
    {
        RemoveOldCache();
        if (_callbacks.TryGetValue(id, out var tcs))
        {
            Task.Run(() => tcs.SetResult(data));
        }
        else
        {
            _waitingResults.TryAdd(id, data);
            _waitingResultCache.Enqueue((DateTime.UtcNow, id));
            await _broadcaster.BroadcastMessageAsync(id, data);
        }
    }


    private void InternalSetResult(string id, T data)
    {
        RemoveOldCache();
        if (_callbacks.TryGetValue(id, out var tcs))
        {
            Task.Run(() => tcs.SetResult(data));
        }
        else
        {
            _waitingResults.TryAdd(id, data);
            _waitingResultCache.Enqueue((DateTime.UtcNow, id));
        }
    }

    public void RemoveOldCache()
    {
        while (_waitingResultCache.TryPeek(out var item))
        {
            if (DateTime.UtcNow - item.Item1 > TimeSpan.FromSeconds(1200))
            {
                if (_waitingResultCache.TryDequeue(out var item2))
                {
                    if (item.Item2 != item2.Item2)
                    {
                        //Something went wrong
                        break;
                    }

                    _waitingResults.TryRemove(item2.Item2, out var data);
                }
            }
            else
            {
                break;
            }
        }
    }

    public Type Type => typeof(T);
}

public interface ICallbackHandler
{
    public Type Type { get; }
}