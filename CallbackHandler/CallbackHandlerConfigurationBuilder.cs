namespace CallbackHandler;

public class CallbackHandlerConfigurationBuilder
{
    private readonly Type _forType;
    private TimeSpan _cacheDuration = TimeSpan.FromSeconds(60);
    private Type? _cacheHandler;

    public CallbackHandlerConfigurationBuilder(Type forType)
    {
        _forType = forType;
    }

    public CallbackHandlerConfigurationBuilder WithInternalCacheDuration(TimeSpan cacheDuration)
    {
        _cacheDuration = cacheDuration;
        return this;
    }

    public CallbackHandlerConfigurationBuilder WithCache(ICacheHandler handler)
    {
        if (handler.Type != _forType)
            throw new ArgumentException("Cache handler type does not match the type of the callback handler");
        _cacheHandler = handler.GetType();
        return this;
    }

    public CallbackHandlerConfigurationBuilder WithCacheHandler<T>() where T:ICacheHandler
    {
        _cacheHandler = typeof(T);
        return this;
    }
    
    public CallbackHandlerConfiguration Build()
    {
        return new CallbackHandlerConfiguration(_cacheDuration, _cacheHandler);
    }
}