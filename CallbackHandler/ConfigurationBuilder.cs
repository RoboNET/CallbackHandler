namespace CallbackHandler;

public class ConfigurationBuilder
{
    private readonly Dictionary<Type, CallbackHandlerConfiguration> _waitingTypes = new();

    public ConfigurationBuilder WithType(Type type, Action<CallbackHandlerConfigurationBuilder>? configuration = null)
    {
        var callbackHandlerConfiguration = new CallbackHandlerConfigurationBuilder(type);
        configuration?.Invoke(callbackHandlerConfiguration);
        _waitingTypes.Add(type, callbackHandlerConfiguration.Build());
        return this;
    }

    public ConfigurationBuilder WithType<T>(Action<CallbackHandlerConfigurationBuilder>? configuration = null)
    {
        return WithType(typeof(T), configuration);
    }

    public ConfigurationBuilder WithTypes(IEnumerable<Type> types, Action<CallbackHandlerConfigurationBuilder>? configuration = null)
    {
        foreach (var type in types)
        {
            WithType(type, configuration);
        }

        return this;
    }

    public Configuration Build()
    {
        return new Configuration(_waitingTypes);
    }
}