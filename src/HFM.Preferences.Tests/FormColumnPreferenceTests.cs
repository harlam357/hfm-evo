namespace HFM.Preferences;

[TestFixture]
public class FormColumnPreferenceTests
{
    [TestCase("01,2,True,4", true)]
    [TestCase("02,3,False,5", true)]
    [TestCase("a,2,True,4", false)]
    [TestCase("01,b,True,4", false)]
    [TestCase("01,2,foo,4", false)]
    [TestCase("01,2,True,c", false)]
    [TestCase("too,many,tokens,to,parse", false)]
    public void CanParseFormColumnPreference(string value, bool isValid)
    {
        var preference = FormColumnPreference.Parse(value);
        var constraint = isValid
            ? Is.Not.Null
            : Is.Null;
        Assert.That(preference, constraint);
    }

    [TestCase(1, 2, true, 4, ExpectedResult = "01,2,True,4")]
    [TestCase(2, 3, false, 5, ExpectedResult = "02,3,False,5")]
    public string CanFormatFormColumnPreference(int displayIndex, int width, bool visible, int index) =>
        FormColumnPreference.Format(displayIndex, width, visible, index);
}
