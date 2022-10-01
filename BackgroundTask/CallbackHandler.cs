using System.Collections.Concurrent;
using System.Text.Json;
using StackExchange.Redis;

namespace BackgroundTask;

public class CallbackHandler<T>
{
    private readonly ISubscriber _redis;

    private ConcurrentDictionary<string, TaskCompletionSource<T>> _callbacks =
        new();

    private ConcurrentDictionary<string, T> _waitingResults = new();

    private ConcurrentQueue<(DateTime, string)> _waitingResultCache =
        new();

    public CallbackHandler(IConnectionMultiplexer redis)
    {
        _redis = redis.GetSubscriber();
        _redis.Subscribe(typeof(T).Name, (channel, value) =>
        {
            var result = JsonSerializer.Deserialize<MultiplexerResult<T>>(value);
            InternalSetResult(result.Id, result.Data);
        });
    }

    public async Task<T> WaitResult(string id)
    {
        if (_waitingResults.TryGetValue(id, out var data))
        {
            return data;
        }

        var tcs = new TaskCompletionSource<T>();
        _callbacks.TryAdd(id, tcs);
        var result = await tcs.Task;
        return result;
    }

    public async Task SetResult(string id, T data)
    {
        if (_callbacks.TryGetValue(id, out var tcs))
        {
            tcs.SetResult(data);
        }
        else
        {
            _waitingResults.TryAdd(id, data);
            _waitingResultCache.Enqueue((DateTime.UtcNow, id));

            await _redis.PublishAsync(typeof(T).Name, JsonSerializer.Serialize(new MultiplexerResult<T>()
            {
                Id = id,
                Data = data
            }));
        }
    }


    private void InternalSetResult(string id, T data)
    {
        if (_callbacks.TryGetValue(id, out var tcs))
        {
            tcs.SetResult(data);
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
}