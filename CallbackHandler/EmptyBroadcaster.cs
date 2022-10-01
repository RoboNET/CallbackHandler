namespace CallbackHandler;

public class EmptyBroadcaster : IMessageBroadcaster
{
    public Task BroadcastMessageAsync<T>(string id, T message)
    {
        return Task.CompletedTask;
    }

    public void Subscribe<T>(Action<string, T> action)
    {
    }
}