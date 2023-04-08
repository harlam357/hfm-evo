using System.Runtime.Serialization;

namespace HFM.Core.Serializers;

[TestFixture]
public class DataContractSerializerTests
{
    [Test]
    public void RoundTrip()
    {
        var serializer = new DataContractSerializer<TheTypeToRoundTrip>();
        var expected = new TheTypeToRoundTrip { Id = 1, Name = "Foo" };
        using var stream = new MemoryStream();
        serializer.Serialize(stream, expected);
        stream.Position = 0;
        var actual = serializer.Deserialize(stream);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [DataContract]
    private sealed record TheTypeToRoundTrip
    {
        [DataMember]
        public int Id { get; init; }

        [DataMember]
        public string? Name { get; init; }
    }
}
