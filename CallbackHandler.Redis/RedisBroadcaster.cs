using System.Text.Json;
using StackExchange.Redis;

namespace CallbackHandler.Redis;

public class RedisBroadcaster : IMessageBroadcaster
{
    private readonly ISubscriber _redis;

    public RedisBroadcaster(IConnectionMultiplexer redis)
    {
        _redis = redis.GetSubscriber();
    }

    public async Task BroadcastMessageAsync<T>(string id, T message)
    {
        await _redis.PublishAsync(typeof(T).Name, JsonSerializer.Serialize(new MultiplexerResult<T>()
        {
            Id = id,
            Data = message
        }));
    }

    public void Subscribe<T>(Action<string, T> action)
    {
        _redis.Subscribe(typeof(T).Name, (channel, value) =>
        {
            var result = JsonSerializer.Deserialize<MultiplexerResult<T>>(value);
            action(result.Id, result.Data);
        });
    }

    class MultiplexerResult<T>
    {
        public string Id { get; set; }
        public T Data { get; set; }
    }
}