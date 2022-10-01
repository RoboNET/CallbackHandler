namespace CallbackHandler;

public class ConfigurationBuilder
{
    private readonly List<Type> _waitingTypes = new List<Type>();

    public ConfigurationBuilder WithType(Type type)
    {
        _waitingTypes.Add(type);
        return this;
    }

    public ConfigurationBuilder WithTypes(IEnumerable<Type> types)
    {
        _waitingTypes.AddRange(types);
        return this;
    }

    public Configuration Build()
    {
        return new Configuration(_waitingTypes);
    }
}