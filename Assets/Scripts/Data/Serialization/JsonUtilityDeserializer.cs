using UnityEngine;

public class JsonUtilityDeserializer : IJsonDeserializer
{
    public bool TryDeserialize<T>(string json, out T result)
    {
        try
        {
            result = JsonUtility.FromJson<T>(json);
            return result != null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"JsonUtilityDeserializer: failed to parse JSON: {e.Message}");
            result = default;
            return false;
        }
    }
}
