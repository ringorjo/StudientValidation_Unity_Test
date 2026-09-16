public class FakeJsonDeserializer : IJsonDeserializer
{
    private readonly object _resultToReturn;
    private readonly bool _shouldSucceed;

    public FakeJsonDeserializer(object resultToReturn, bool shouldSucceed = true)
    {
        _resultToReturn = resultToReturn;
        _shouldSucceed = shouldSucceed;
    }

    public bool TryDeserialize<T>(string json, out T result)
    {
        if (_shouldSucceed && _resultToReturn is T typed)
        {
            result = typed;
            return true;
        }

        result = default;
        return false;
    }
}
