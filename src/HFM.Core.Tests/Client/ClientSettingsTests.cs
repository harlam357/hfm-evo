namespace HFM.Core.Client;

[TestFixture]
public class ClientSettingsTests
{
    [Test]
    public void HasClientIdentifier()
    {
        var guid = Guid.NewGuid();
        var settings = new ClientSettings
        {
            ClientType = ClientType.FahClient,
            Name = "Foo",
            Server = "Bar",
            Port = 12345,
            Guid = guid
        };

        var expected = new ClientIdentifier("Foo", "Bar", 12345, guid);
        Assert.That(settings.ClientIdentifier, Is.EqualTo(expected));
    }

    [Test]
    public void HasClientLogFileName()
    {
        var settings = new ClientSettings
        {
            Name = "Foo",
        };

        const string expected = "Foo-log.txt";
        Assert.That(settings.ClientLogFileName, Is.EqualTo(expected));
    }

    [TestCase("+a+", true)]
    [TestCase("=a=", true)]
    [TestCase("-a-", true)]
    [TestCase("_a_", true)]
    [TestCase("$a$", true)]
    [TestCase("&a&", true)]
    [TestCase("^a^", true)]
    [TestCase("[a[", true)]
    [TestCase("]a]", true)]
    [TestCase("}a}", false)]
    [TestCase("\\a\\", false)]
    [TestCase("|a|", false)]
    [TestCase(";a;", false)]
    [TestCase(":a:", false)]
    [TestCase("\'a\'", false)]
    [TestCase("\"a\"", false)]
    [TestCase(",a,", false)]
    [TestCase("<a<", false)]
    [TestCase(">a>", false)]
    [TestCase("/a/", false)]
    [TestCase("?a?", false)]
    [TestCase("`a`", false)]
    [TestCase("~a~", false)]
    [TestCase("!a!", false)]
    [TestCase("@a@", false)]
    [TestCase("#a#", false)]
    [TestCase("%a%", false)]
    [TestCase("*a*", false)]
    [TestCase("(a(", false)]
    [TestCase(")a)", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void ValidatesClientName(string? name, bool expected) =>
        Assert.That(ClientSettings.ValidateName(name), Is.EqualTo(expected));
}
