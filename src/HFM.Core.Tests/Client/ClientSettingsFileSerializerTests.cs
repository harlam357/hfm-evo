using System.Runtime.Serialization;
using System.Xml.Linq;

namespace HFM.Core.Client;

[TestFixture]
public class ClientSettingsFileSerializerTests
{
    private ClientSettingsFileSerializer? _serializer;

    [SetUp]
    public virtual void BeforeEach() => _serializer = new ClientSettingsFileSerializer();

    [TestFixture]
    public class GivenClientSettingsFileSerializer : ClientSettingsFileSerializerTests
    {
        [Test]
        public void HasFileExtension() => Assert.That(_serializer!.FileExtension, Is.Not.Null);

        [Test]
        public void HasFileTypeFilter() => Assert.That(_serializer!.FileTypeFilter, Is.Not.Null);

        [Test]
        public void SerializesNull()
        {
            using var artifacts = new ArtifactFolder();
            Assert.That(() => _serializer!.Serialize(artifacts.GetRandomFilePath(), null), Throws.Nothing);
        }
    }

    [TestFixture]
    public class GivenExistingEmptyClientSettingsFile : ClientSettingsFileSerializerTests, IDisposable
    {
        private ArtifactFolder? _artifacts;
        private string? _path;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _artifacts = new ArtifactFolder();
            _path = _artifacts.GetRandomFilePath();
            using var stream = File.Create(_path);
            stream.Close();
        }

        [TearDown]
        public void AfterEach() => _artifacts?.Dispose();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _artifacts?.Dispose();
        }

        [Test]
        public void ThrowsSerializationException() =>
            Assert.That(() => _serializer!.Deserialize(_path!), Throws.TypeOf<SerializationException>());
    }

    [TestFixture]
    public class GivenExistingClientSettingsFile : ClientSettingsFileSerializerTests
    {
        private List<ClientSettings>? _settingsCollection;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            using var artifacts = new ArtifactFolder();
            string path = artifacts.GetRandomFilePath();
            File.Copy("TestFiles\\ClientSettings_0_9_11.hfmx", path);
            _settingsCollection = _serializer!.Deserialize(path);
        }

        [Test]
        public void Deserializes() =>
            Assert.Multiple(() =>
            {
                Assert.That(_settingsCollection, Is.Not.Null);
                Assert.That(_settingsCollection, Has.Count.EqualTo(1));

                var s = _settingsCollection!.First();
                Assert.That(s.ClientType, Is.EqualTo(ClientType.FahClient));
                Assert.That(s.Name, Is.EqualTo("Client1"));
                Assert.That(s.Server, Is.EqualTo("192.168.100.250"));
                Assert.That(s.Port, Is.EqualTo(36330));
                Assert.That(s.Password, Is.EqualTo("foobar"));
                Assert.That(s.Guid, Is.Not.EqualTo(Guid.Empty));
            });
    }

    [TestFixture]
    public class GivenExistingClientSettingsFileBadPasswordFormat : ClientSettingsFileSerializerTests
    {
        private List<ClientSettings>? _settingsCollection;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            using var artifacts = new ArtifactFolder();
            string path = artifacts.GetRandomFilePath();
            File.Copy("TestFiles\\ClientSettings_0_9_11.hfmx", path);

            var settingsRootNode = XElement.Load(path);
            foreach (var item in settingsRootNode.Descendants("ClientSettings"))
            {
                item.Element("Password")!.Value = "foobar";
            }
            using (var stream = new FileStream(path, FileMode.Create))
            {
                settingsRootNode.Save(stream);
            }

            _settingsCollection = _serializer!.Deserialize(path);
        }

        [Test]
        public void ReadsClearPasswordValue()
        {
            var s = _settingsCollection!.First();
            Assert.That(s.Password, Is.EqualTo("foobar"));
        }
    }

    [TestFixture]
    public class RoundTripToFromFile : ClientSettingsFileSerializerTests
    {
        private List<ClientSettings>? _toFile;
        private List<ClientSettings>? _fromFile;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var guid = Guid.NewGuid();
            _toFile = new List<ClientSettings>
            {
                new()
                {
                    ClientType = ClientType.FahClient,
                    Name = "Foo",
                    Server = "Bar",
                    Port = 12345,
                    Password = "fizzbizz",
                    Guid = guid
                }
            };

            using var artifacts = new ArtifactFolder();
            string path = artifacts.GetRandomFilePath();

            _serializer!.Serialize(path, _toFile);
            _fromFile = _serializer!.Deserialize(path);
        }

        [Test]
        public void AreEqual() =>
            Assert.Multiple(() =>
            {
                Assert.That(_fromFile, Has.Count.EqualTo(_toFile!.Count));
                var to = _toFile.First();
                var from = _fromFile!.First();
                Assert.That(from, Is.EqualTo(to));
            });
    }

    [TestFixture]
    public class GivenClientSettingsWithoutGuidValues : ClientSettingsFileSerializerTests
    {
        private List<ClientSettings>? _settingsCollection;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            using var artifacts = new ArtifactFolder();
            string path = artifacts.GetRandomFilePath();
            var toFile = new List<ClientSettings> { new() };
            _serializer!.Serialize(path, toFile);
            _settingsCollection = _serializer.Deserialize(path);
        }

        [Test]
        public void SerializeGeneratesGuidValuesWhenGuidIsEmpty() =>
            Assert.That(_settingsCollection!.All(x => x.Guid != Guid.Empty), Is.True);
    }
}
