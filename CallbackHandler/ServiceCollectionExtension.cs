using Microsoft.Extensions.DependencyInjection;

namespace CallbackHandler;

public static class ServiceCollectionExtension
{
    public static void RegisterCallbackHandler(this IServiceCollection serviceCollection,
        Action<ConfigurationBuilder> configurationFunc)
    {
        serviceCollection.AddSingleton<IMessageBroadcaster, EmptyBroadcaster>();
        
        var configurationBuilder = new ConfigurationBuilder();
        configurationFunc(configurationBuilder);
        var configuration = configurationBuilder.Build();
        Type generic = typeof(CallbackHandler<>);
            
        foreach (var type in configuration.Types)
        {
            Type[] typeArgs = {type.Key};
            Type constructed = generic.MakeGenericType(typeArgs);

            serviceCollection.AddSingleton(constructed, provider =>
            {
                var messageBroadcaster = provider.GetRequiredService<IMessageBroadcaster>();
                var instance = Activator.CreateInstance(constructed, messageBroadcaster, type.Value, provider);
                if (instance == null)
                    throw new NullReferenceException("CallbackHandler instance is null");
                return instance;
            });
        }
    }
}