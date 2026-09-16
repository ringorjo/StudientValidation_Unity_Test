using NUnit.Framework;

public class JsonUtilityDeserializerTests
{
    private class SamplePayload
    {
        public string valor;
    }

    [Test]
    public void TryDeserialize_ValidJson_ReturnsTrueAndPopulatedObject()
    {
        var deserializer = new JsonUtilityDeserializer();

        bool ok = deserializer.TryDeserialize("{\"valor\":\"hola\"}", out SamplePayload result);

        Assert.IsTrue(ok);
        Assert.AreEqual("hola", result.valor);
    }

    [Test]
    public void TryDeserialize_InvalidJson_ReturnsFalse()
    {
        var deserializer = new JsonUtilityDeserializer();

        bool ok = deserializer.TryDeserialize("{esto no es json", out SamplePayload result);

        Assert.IsFalse(ok);
    }
}
