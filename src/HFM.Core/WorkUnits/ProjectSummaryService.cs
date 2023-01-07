using HFM.Preferences;
using HFM.Proteins;

namespace HFM.Core.WorkUnits;

public interface IProjectSummaryService
{
    Task<ICollection<Protein>?> GetProteins();
}

public class ProjectSummaryService : IProjectSummaryService
{
    private static HttpClient? _HttpClient;

    private readonly IPreferences _preferences;

    public ProjectSummaryService(IPreferences preferences)
    {
        _HttpClient ??= CreateHttpClient();

        _preferences = preferences;
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.CacheControl = new()
        {
            NoCache = true
        };
        return httpClient;
    }

    public async Task<ICollection<Protein>?> GetProteins()
    {
        var requestUri = new Uri(_preferences.Get<string>(Preference.ProjectDownloadUrl)!);
        var response = await _HttpClient!.GetAsync(requestUri).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using (stream)
        {
            var serializer = new ProjectSummaryJsonDeserializer();
            return await serializer.DeserializeAsync(stream).ConfigureAwait(false);
        }
    }
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullProjectSummaryService : IProjectSummaryService
{
    public static NullProjectSummaryService Instance { get; } = new();

    public Task<ICollection<Protein>?> GetProteins() => Task.FromResult<ICollection<Protein>?>(null);
}
