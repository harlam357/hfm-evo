using HFM.Core.Logging;

using Microsoft.Extensions.DependencyInjection;

namespace HFM.Core.Client;

[TestFixture]
public class ClientFactoryTests
{
    private ClientFactory? _factory;

    [SetUp]
    public void BeforeEach()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(NullLogger.Instance);
        services.AddTransient<FahClient>();
        var serviceProvider = services.BuildServiceProvider();

        _factory = new ClientFactory(serviceProvider);
    }

    [Test]
    public void CreatesFahClient()
    {
        var settings = new ClientSettings
        {
            Name = "foo",
            Server = "bar"
        };
        var client = _factory!.Create(settings);
        Assert.Multiple(() =>
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(client, Is.TypeOf<FahClient>());
            Assert.That(client!.Settings, Is.Not.Null);
        });
    }

    [Test]
    public void ReturnsNullWhenClientTypeIsUnknown()
    {
        var settings = new ClientSettings
        {
            ClientType = (ClientType)Int32.MaxValue,
            Name = "foo",
            Server = "bar"
        };
        var client = _factory!.Create(settings);
        Assert.That(client, Is.Null);
    }

    [TestCase("foo", "bar", 0)]
    [TestCase("", "bar", ClientSettings.DefaultPort)]
    [TestCase("foo", "", ClientSettings.DefaultPort)]
    [TestCase("<foo>", "bar", ClientSettings.DefaultPort)]
    public void ThrowsWhenClientSettingsIsNotValid(string name, string server, int port)
    {
        var settings = new ClientSettings
        {
            Name = name,
            Server = server,
            Port = port
        };
        Assert.That(() => _factory!.Create(settings), Throws.ArgumentException);
    }
}
