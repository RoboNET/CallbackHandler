using CallbackHandler;

namespace BackgroundTask;

public class AuthResult
{
    public string Id { get; set; }
    public bool Result { get; set; }
}

public class AuthResultHandler : ICacheHandler<AuthResult>
{
    public async Task<AuthResult> GetResult(string id)
    {
        return new AuthResult()
        {
            Id = id,
            Result = true
        };
    }
}