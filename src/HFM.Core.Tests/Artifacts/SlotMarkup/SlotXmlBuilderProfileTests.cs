using AutoMapper;

namespace HFM.Core.Artifacts.SlotMarkup;

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
