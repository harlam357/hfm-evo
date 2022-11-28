namespace HFM.Core.Internal;

[TestFixture]
public class CryptographyTests
{
    private Cryptography? _cryptography;

    [SetUp]
    public void BeforeEach()
    {
        const string initializationVector = "3k1vKL=Cz6!wZS`I";
        const string symmetricKey = "%`Bb9ega;$.GUDaf";
        _cryptography = new Cryptography(symmetricKey, initializationVector);
    }

    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("   ", ExpectedResult = "")]
    [TestCase("fizzbizz", ExpectedResult = "8YsqRpczouPuCPApFum1YQ==")]
    public string EncryptsValue(string? value) => _cryptography!.EncryptValue(value);

    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("   ", ExpectedResult = "")]
    [TestCase("8YsqRpczouPuCPApFum1YQ==", ExpectedResult = "fizzbizz")]
    [TestCase("*notbase64encoded*", ExpectedResult = "*notbase64encoded*")]
    [TestCase("notencrypted", ExpectedResult = "notencrypted")]
    public string DecryptsValue(string? value) => _cryptography!.DecryptValue(value);
}
