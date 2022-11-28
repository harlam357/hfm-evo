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

    [TestCase("+a+", ExpectedResult = true)]
    [TestCase("=a=", ExpectedResult = true)]
    [TestCase("-a-", ExpectedResult = true)]
    [TestCase("_a_", ExpectedResult = true)]
    [TestCase("$a$", ExpectedResult = true)]
    [TestCase("&a&", ExpectedResult = true)]
    [TestCase("^a^", ExpectedResult = true)]
    [TestCase("[a[", ExpectedResult = true)]
    [TestCase("]a]", ExpectedResult = true)]
    [TestCase("}a}", ExpectedResult = false)]
    [TestCase("\\a\\", ExpectedResult = false)]
    [TestCase("|a|", ExpectedResult = false)]
    [TestCase(";a;", ExpectedResult = false)]
    [TestCase(":a:", ExpectedResult = false)]
    [TestCase("\'a\'", ExpectedResult = false)]
    [TestCase("\"a\"", ExpectedResult = false)]
    [TestCase(",a,", ExpectedResult = false)]
    [TestCase("<a<", ExpectedResult = false)]
    [TestCase(">a>", ExpectedResult = false)]
    [TestCase("/a/", ExpectedResult = false)]
    [TestCase("?a?", ExpectedResult = false)]
    [TestCase("`a`", ExpectedResult = false)]
    [TestCase("~a~", ExpectedResult = false)]
    [TestCase("!a!", ExpectedResult = false)]
    [TestCase("@a@", ExpectedResult = false)]
    [TestCase("#a#", ExpectedResult = false)]
    [TestCase("%a%", ExpectedResult = false)]
    [TestCase("*a*", ExpectedResult = false)]
    [TestCase("(a(", ExpectedResult = false)]
    [TestCase(")a)", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(null, ExpectedResult = false)]
    public bool ValidatesClientName(string? name) => ClientSettings.ValidateName(name);
}
