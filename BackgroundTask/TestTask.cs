using CallbackHandler;

namespace BackgroundTask;

public class TestTask
{
    private readonly CallbackHandler<AuthResult> _authHandler;
    private readonly CallbackHandler<ConfirmResult> _confirmHandler;
    private readonly ILogger<TestTask> _logger;

    public static int Result = 0;

    public TestTask(CallbackHandler<AuthResult> authHandler, CallbackHandler<ConfirmResult> confirmHandler, ILogger<TestTask> logger)
    {
        _authHandler = authHandler;
        _confirmHandler = confirmHandler;
        _logger = logger;
    }

    public async Task Process(string id)
    {
        try
        {
            //make auth request
            await Task.Delay(10000);
            var authResult = await _authHandler.WaitResult(id);
            if (authResult.Id != id)
            {
                throw new Exception("Fail");
            }

            //Что-то продолжаем делать
            await Task.Delay(10000);
            var confirmResult = await _confirmHandler.WaitResult(id);
            if (confirmResult.Id != id)
            {
                throw new Exception("Fail 2");
            }

            Interlocked.Increment(ref Result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Processing error");
            throw;
        }
    }
}