namespace CallbackHandler;

public interface IMessageBroadcaster
{
    Task BroadcastMessageAsync<T>(string id, T message);
    
    void Subscribe<T>(Action<string, T> action);
}