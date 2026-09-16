public interface IJsonDeserializer
{
    bool TryDeserialize<T>(string json, out T result);
}
