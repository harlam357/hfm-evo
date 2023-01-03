using AutoMapper;

namespace HFM.Core.Artifacts.SlotXml;

[TestFixture]
public class SlotXmlBuilderProfileTests
{
    [Test]
    public void AssertConfigurationIsValid()
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<SlotXmlBuilderProfile>());
        configuration.AssertConfigurationIsValid();
    }
}
