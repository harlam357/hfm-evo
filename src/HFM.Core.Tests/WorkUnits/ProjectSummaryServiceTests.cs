using HFM.Preferences;

namespace HFM.Core.WorkUnits;

[TestFixture]
public class ProjectSummaryServiceTests
{
    [Test]
    [Category(TestCategory.Integration)]
    public async Task GetsProjectSummary()
    {
        var preferences = new InMemoryPreferencesProvider();
        var service = new ProjectSummaryService(preferences);
        var proteins = await service.GetProteins();
        Assert.That(proteins, Has.Count.Not.EqualTo(0));
    }
}
