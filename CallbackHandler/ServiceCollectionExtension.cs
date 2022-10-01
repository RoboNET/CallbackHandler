using Microsoft.Extensions.DependencyInjection;

namespace CallbackHandler;

public static class ServiceCollectionExtension
{
    public static void RegisterCallbackHandler(this IServiceCollection serviceCollection,
        Action<ConfigurationBuilder> configurationFunc)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationFunc(configurationBuilder);
        var configuration = configurationBuilder.Build();
        Type generic = typeof(CallbackHandler<>);


        foreach (var type in configuration.Types)
        {
            Type[] typeArgs = {type};

            Type constructed = generic.MakeGenericType(typeArgs);
            serviceCollection.AddSingleton(constructed);
        }
    }
}