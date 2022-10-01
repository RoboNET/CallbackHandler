namespace CallbackHandler;

public class CallbackHandlerFactory
{
    private readonly Dictionary<Type, ICallbackHandler> _handlers = new();

    public CallbackHandlerFactory(IEnumerable<ICallbackHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            _handlers.Add(handler.GetType(), handler);
        }
    }

    public CallbackHandler<T> GetHandler<T>()
    {
        return (CallbackHandler<T>) _handlers[typeof(T)];
    }
}