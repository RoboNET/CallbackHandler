using System.Collections.Concurrent;

namespace CallbackHandler;

public class CallbackHandler<T> : ICallbackHandler
{
    private readonly IMessageBroadcaster _broadcaster;
    private readonly Configuration _configuration;

    private ConcurrentDictionary<string, TaskCompletionSource<T>> _callbacks =
        new();

    private ConcurrentDictionary<string, T> _waitingResults = new();

    private ConcurrentQueue<(DateTime, string)> _waitingResultCache =
        new();

    public CallbackHandler(IMessageBroadcaster broadcaster, Configuration configuration)
    {
        _broadcaster = broadcaster;
        _configuration = configuration;
        _broadcaster.Subscribe((string id, T message) => { InternalSetResult(id, message); });
    }

    public async Task<T> WaitResult(string id, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
    {
        if (_waitingResults.TryGetValue(id, out var data))
        {
            return data;
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
            var result = await tcs.Task;
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
                        //Какой-то пиздец
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