using HFM.Core.Logging;
using HFM.Preferences;
using HFM.Proteins;

using Moq;

namespace HFM.Core.WorkUnits;

[TestFixture]
public class ProteinServiceTests
{
    private ProteinService? _service;
    private IProjectSummaryService? _projectSummaryService;

    [TestFixture]
    public class GivenServiceHasProteins : ProteinServiceTests
    {
        [SetUp]
        public virtual void BeforeEach()
        {
            var dataContainer = new ProteinDataContainer(new InMemoryPreferencesProvider())
            {
                Data = Enumerable.Range(1, 5).Select(CreateValidProtein).ToList()
            };
            _projectSummaryService = CreateProjectSummaryService();
            _service = new ProteinService(dataContainer, _projectSummaryService, TestLogger.Instance);
        }

        [Test]
        public void ThenGetAvailableProtein()
        {
            var p = _service!.Get(1);
            Assert.That(p, Is.Not.Null);
        }

        [Test]
        public void ThenGetUnavailableProtein()
        {
            var p = _service!.Get(2482);
            Assert.That(p, Is.Null);
        }

        [Test]
        public async Task ThenGetOrRefreshAvailableProtein()
        {
            var p = await _service!.GetOrRefresh(1);
            Assert.That(p, Is.Not.Null);
        }

        [Test]
        public async Task ThenUnavailableProteinIsAddedToLastProjectRefreshList()
        {
            await _service!.GetOrRefresh(2482);
            Assert.That(_service.LastProjectRefresh.ContainsKey(2482), Is.True);
        }

        [Test]
        public async Task ThenByLastRefreshUnavailableProteinTriggersOneCallToProjectSummaryService()
        {
            var p = await _service!.GetOrRefresh(2482);
            Assert.That(p, Is.Null);
            p = await _service!.GetOrRefresh(2482);
            Assert.That(p, Is.Null);

            Mock.Get(_projectSummaryService).Verify(x => x!.GetProteins(), Times.Once);
        }

        [Test]
        public async Task ThenByLastProjectRefreshUnavailableProteinTriggersOneCallToProjectSummaryService()
        {
            var p = await _service!.GetOrRefresh(2482);
            Assert.That(p, Is.Null);

            _service.LastRefresh = null;
            p = await _service!.GetOrRefresh(2482);
            Assert.That(p, Is.Null);

            Mock.Get(_projectSummaryService).Verify(x => x!.GetProteins(), Times.Once);
        }

        [Test]
        public async Task ThenGetOrRefreshAllowsRefreshWhenLastRefreshHasElapsed()
        {
            _service!.LastRefresh = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(61));
            var p = await _service.GetOrRefresh(6940);
            Assert.That(p, Is.Not.Null);

            Mock.Get(_projectSummaryService).Verify(x => x!.GetProteins(), Times.Once);
        }

        [Test]
        public async Task ThenGetOrRefreshRemovesTheProjectFromLastProjectRefresh()
        {
            _service!.LastProjectRefresh.Add(6940, DateTime.MinValue);
            var p = await _service.GetOrRefresh(6940);
            Assert.Multiple(() =>
            {
                Assert.That(p, Is.Not.Null);
                Assert.That(_service.LastProjectRefresh.ContainsKey(6940), Is.False);
            });
        }

        [Test]
        public void ThenItReturnsListOfProjectNumbers()
        {
            var projects = _service!.GetProjects();
            Assert.That(projects.SequenceEqual(Enumerable.Range(1, 5)));
        }

        [Test]
        public async Task ThenRefreshRemovesTheProjectFromLastProjectRefresh()
        {
            _service!.LastProjectRefresh.Add(6940, DateTime.MinValue);
            await _service.Refresh();
            Assert.That(_service.LastProjectRefresh.ContainsKey(6940), Is.False);
        }

        [Test]
        public async Task ThenRefreshUpdatesLastRefreshValues()
        {
            _service!.LastProjectRefresh.Add(2968, DateTime.MinValue);
            _service.LastRefresh = DateTime.MinValue;
            await _service.Refresh();
            Assert.Multiple(() =>
            {
                Assert.That(_service.LastProjectRefresh[2968], Is.Not.EqualTo(DateTime.MinValue));
                Assert.That(_service.LastRefresh, Is.Not.EqualTo(DateTime.MinValue));
            });
        }

        [Test]
        public async Task ThenRefreshUpdatesProjects()
        {
            int count = _service!.GetProjects().Count;
            await _service.Refresh();
            Assert.That(_service.GetProjects(), Has.Count.Not.EqualTo(count));
        }

        [Test]
        public async Task ThenRefreshReturnsProteinChanges()
        {
            var changes = await _service!.Refresh();
            Assert.That(changes, Has.Count.EqualTo(624));
        }
    }

    [TestFixture]
    public class GivenProjectSummaryServiceThrows : ProteinServiceTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            var dataContainer = new ProteinDataContainer(new InMemoryPreferencesProvider());
            _projectSummaryService = CreateProjectSummaryServiceThatThrows();
            _logger = Mock.Of<ILogger>();
            _service = new ProteinService(dataContainer, _projectSummaryService, _logger);
        }

        [Test]
        public async Task ThenGetOrRefreshReturnsNull()
        {
            var changes = await _service!.GetOrRefresh(2482);
            Assert.That(changes, Is.Null);
        }

        [Test]
        public async Task ThenGetOrRefreshLogsTheException()
        {
            await _service!.GetOrRefresh(2482);

            Mock.Get(_logger).Verify(x => x!.Log(LoggerLevel.Error, It.IsAny<string>(), It.IsNotNull<Exception>()), Times.Once);
        }
    }

    private static IProjectSummaryService CreateProjectSummaryService()
    {
        string path = Path.Combine(TestFiles.SolutionPath, "summary.json");
        var proteins = new ProjectSummaryJsonDeserializer().Deserialize(File.OpenRead(path));

        var mockSummaryService = new Mock<IProjectSummaryService>();
        mockSummaryService.Setup(x => x.GetProteins()).Returns(Task.FromResult<ICollection<Protein>?>(proteins));
        return mockSummaryService.Object;
    }

    private static IProjectSummaryService CreateProjectSummaryServiceThatThrows()
    {
        var mockSummaryService = new Mock<IProjectSummaryService>();
        mockSummaryService.Setup(x => x.GetProteins()).ThrowsAsync(new Exception("foo"));
        return mockSummaryService.Object;
    }

    private static Protein CreateValidProtein(int projectNumber) =>
        new()
        {
            ProjectNumber = projectNumber,
            PreferredDays = 1,
            MaximumDays = 1,
            Credit = 1,
            Frames = 100
        };
}
