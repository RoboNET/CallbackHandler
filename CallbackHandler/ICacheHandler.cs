namespace CallbackHandler;

public interface ICacheHandler<T> : ICacheHandler
{
    Task<T> GetResult(string id);
    Type ICacheHandler.Type => typeof(T);
}

public interface ICacheHandler
{
    Type Type { get; }
}