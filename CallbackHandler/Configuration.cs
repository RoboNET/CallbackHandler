namespace CallbackHandler;

public record Configuration(Dictionary<Type, CallbackHandlerConfiguration> Types);

public record CallbackHandlerConfiguration(TimeSpan CacheDuration, Type? CacheHandler = null);