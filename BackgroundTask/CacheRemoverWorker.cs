using CallbackHandler;

namespace BackgroundTask;

public class CacheRemoverWorker : BackgroundService
{
    private readonly CallbackHandler<AuthResult> _authHandler;
    private readonly CallbackHandler<ConfirmResult> _confirmHandler;

    public CacheRemoverWorker(CallbackHandler<AuthResult> authHandler, CallbackHandler<ConfirmResult> confirmHandler)
    {
        _authHandler = authHandler;
        _confirmHandler = confirmHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _authHandler.RemoveOldCache();
            _confirmHandler.RemoveOldCache();
            await Task.Delay(10000);
        }
    }
}