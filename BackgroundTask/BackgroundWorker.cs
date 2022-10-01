using System.Collections.Concurrent;

namespace BackgroundTask;

public class BackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public static ConcurrentQueue<string> ProcessTasksQueue = new();
    private readonly ConcurrentBag<Task> _processingTasks = new();

    public BackgroundWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (ProcessTasksQueue.TryDequeue(out var id))
            {
                var task = Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var testTask = scope.ServiceProvider.GetRequiredService<TestTask>();
                    await testTask.Process(id);
                }, stoppingToken);
                _processingTasks.Add(task);
            }
            else
            {
                await Task.Delay(10, stoppingToken);
            }
        }

        await Task.WhenAll(_processingTasks);
    }
}