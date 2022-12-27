using HFM.Core.Logging;
using HFM.Proteins;

namespace HFM.Core.WorkUnits;

public interface IProteinService
{
    /// <summary>
    /// Gets the protein with the given project ID.
    /// </summary>
    /// <param name="projectId">The project ID of the protein to return.</param>
    /// <returns>The protein object with the given project ID or null if the protein does not exist.</returns>
    Protein? Get(int projectId);

    /// <summary>
    /// Gets the protein with the given project ID or refreshes the service data from the project summary.
    /// </summary>
    /// <param name="projectId">The project ID of the protein to return.</param>
    /// <returns>The protein object with the given project ID or null if the protein does not exist.</returns>
    Task<Protein?> GetOrRefresh(int projectId);

    /// <summary>
    /// Gets a collection of all protein project ID numbers.
    /// </summary>
    /// <returns>A collection of all protein project ID numbers.</returns>
    ICollection<int> GetProjects();

    /// <summary>
    /// Refreshes the service data from the project summary and returns a collection of objects detailing how the service data was changed.
    /// </summary>
    /// <returns>A collection of objects detailing how the service data was changed</returns>
    Task<IReadOnlyCollection<ProteinChange>?> Refresh();
}

public class ProteinService : IProteinService
{
    private readonly ProteinDataContainer _dataContainer;
    private readonly IProjectSummaryService _projectSummaryService;
    private readonly ILogger _logger;
    private ProteinCollection _collection;

    public ProteinService(ProteinDataContainer dataContainer, IProjectSummaryService projectSummaryService, ILogger? logger)
    {
        _dataContainer = dataContainer;
        _projectSummaryService = projectSummaryService;
        _logger = logger ?? NullLogger.Instance;
        _collection = CreateProteinCollection(_dataContainer.Data);

        LastProjectRefresh = new Dictionary<int, DateTime>();
    }

    private static ProteinCollection CreateProteinCollection(IEnumerable<Protein>? proteins)
    {
        var collection = new ProteinCollection();
        if (proteins is not null)
        {
            foreach (var p in proteins)
            {
                collection.Add(p);
            }
        }
        return collection;
    }

    public Protein? Get(int projectId) => _collection.TryGetValue(projectId, out Protein p) ? p : null;

    public ICollection<int> GetProjects() => _collection.Select(x => x.ProjectNumber).ToList();

    public async Task<Protein?> GetOrRefresh(int projectId)
    {
        if (_collection.TryGetValue(projectId, out Protein p))
        {
            return p;
        }

        if (ProjectIdIsNotValid(projectId) || AutoRefreshIsNotAvailable(projectId))
        {
            return null;
        }

        _logger.Info($"Project ID {projectId} triggering project data refresh.");
        try
        {
            await Refresh().ConfigureAwait(false);
            if (_collection.TryGetValue(projectId, out p))
            {
                return p;
            }
            LastProjectRefresh[projectId] = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
        }

        return null;
    }

    private static bool ProjectIdIsNotValid(int projectId) => projectId <= 0;

    public async Task<IReadOnlyCollection<ProteinChange>?> Refresh()
    {
        _logger.Info("Downloading new project data...");
        var proteins = await _projectSummaryService.GetProteins().ConfigureAwait(false);

        IReadOnlyCollection<ProteinChange>? changes = null;
        if (proteins is not null)
        {
            var collection = new ProteinCollection(_collection);
            changes = collection.Update(proteins);
            Interlocked.Exchange(ref _collection, collection);

            foreach (var info in changes.Where(info => info.Action != ProteinChangeAction.None))
            {
                _logger.Info(info.ToString());
            }
        }

        var now = DateTime.UtcNow;
        foreach (var key in LastProjectRefresh.Keys.ToList())
        {
            if (_collection.Contains(key))
            {
                LastProjectRefresh.Remove(key);
            }
            else
            {
                LastProjectRefresh[key] = now;
            }
        }
        LastRefresh = now;

        Write();
        return changes;
    }

    private void Write()
    {
        _dataContainer.Data = _collection.ToList();
        _dataContainer.Write();
    }

    /// <summary>
    /// Gets the dictionary that contains previously queried project ID numbers that were not found and the date and time of the query.
    /// </summary>
    internal IDictionary<int, DateTime> LastProjectRefresh { get; }

    internal DateTime? LastRefresh { get; set; }

    private bool AutoRefreshIsNotAvailable(int projectId)
    {
        var utcNow = DateTime.UtcNow;

        if (LastRefresh.HasValue)
        {
            const double canRefreshAfterHours = 1.0;
            TimeSpan lastRefreshDifference = utcNow.Subtract(LastRefresh.Value);
            if (lastRefreshDifference.TotalHours < canRefreshAfterHours)
            {
                _logger.Debug($"Refresh executed {lastRefreshDifference.TotalMinutes:0} minutes ago.");
                return true;
            }
        }

        if (LastProjectRefresh.TryGetValue(projectId, out DateTime value))
        {
            const double canRefreshAfterHours = 24.0;
            TimeSpan lastRefreshDifference = utcNow.Subtract(value);
            if (lastRefreshDifference.TotalHours < canRefreshAfterHours)
            {
                _logger.Debug($"Project {projectId} refresh executed {lastRefreshDifference.TotalMinutes:0} minutes ago.");
                return true;
            }
        }

        return false;
    }
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class NullProteinService : IProteinService
{
    public static NullProteinService Instance { get; } = new();

    public Protein? Get(int projectId) => null;

    public Task<Protein?> GetOrRefresh(int projectId) => Task.FromResult<Protein?>(null);

    public ICollection<int> GetProjects() => new List<int>(0);

    public Task<IReadOnlyCollection<ProteinChange>?> Refresh() => Task.FromResult<IReadOnlyCollection<ProteinChange>?>(null);
}
