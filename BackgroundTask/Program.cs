using BackgroundTask;
using Microsoft.AspNetCore.Hosting.Server.Features;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CallbackHandler<AuthResult>>();
builder.Services.AddSingleton<CallbackHandler<ConfirmResult>>();
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect("127.0.0.1:6379"));
builder.Services.AddTransient<TestTask>();
builder.Services.AddHostedService<CacheRemoverWorker>();
builder.Services.AddHostedService<BackgroundWorker>();

var app = builder.Build();

app.MapGet("/test/{id}", async (string id) =>
{
    
    await Task.Delay(1000);
    BackgroundWorker.ProcessTasksQueue.Enqueue(id);
});

app.MapGet("/callback/auth/{id}", async (CallbackHandler<AuthResult> task, string id) =>
{
    
    await task.SetResult(id, new AuthResult()
    {
        Id = id,
        Result = true
    });
});

app.MapGet("/callback/confirm/{id}", async (CallbackHandler<ConfirmResult> task, string id) =>
{
    await task.SetResult(id, new ConfirmResult()
    {
        Id = id,
        Result = true
    });
});

app.MapGet("/result",
    async () =>
    {
        return "Processed:" + TestTask.Result;
    });

app.Run();