using CallbackHandler;

namespace BackgroundTask;

public class TestTask
{
    private readonly CallbackHandler<AuthResult> _authHandler;
    private readonly CallbackHandler<ConfirmResult> _confirmHandler;

    public static int Result=0;

    public TestTask(CallbackHandler<AuthResult> authHandler, CallbackHandler<ConfirmResult> confirmHandler)
    {
        _authHandler = authHandler;
        _confirmHandler = confirmHandler;
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
            throw new Exception("пизда");
        }

        //Что-то продолжаем делать
        await Task.Delay(10000);
        var confirmResult = await _confirmHandler.WaitResult(id);
        if (confirmResult.Id != id)
        {
            throw new Exception("пизда 2");
        }

        Interlocked.Increment(ref Result); }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}