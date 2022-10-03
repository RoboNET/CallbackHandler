namespace CallbackHandler;

public record CallbackHandlerConfiguration(TimeSpan CacheDuration, Type? CacheHandler = null);