using System.Runtime.Serialization;

namespace HFM.Core.Serializers;

[TestFixture]
public class DataContractFileSerializerTests
{
    [Test]
    public void RoundTrip()
    {
        var serializer = new DataContractFileSerializer<RoundTripTest>();
        var expected = new RoundTripTest { Id = 1, Name = "Foo" };
        using var artifacts = new ArtifactFolder();
        string path = artifacts.GetRandomFilePath();
        serializer.Serialize(path, expected);
        var actual = serializer.Deserialize(path);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [DataContract]
    private sealed record RoundTripTest
    {
        [DataMember]
        public int Id { get; init; }

        [DataMember]
        public string? Name { get; init; }
    }

    [Test]
    public void HasFileExtension()
    {
        var serializer = new DataContractFileSerializer<RoundTripTest>();
        Assert.That(serializer.FileExtension, Is.Not.Null);
    }

    [Test]
    public void HasFileTypeFilter()
    {
        var serializer = new DataContractFileSerializer<RoundTripTest>();
        Assert.That(serializer.FileTypeFilter, Is.Not.Null);
    }
}
