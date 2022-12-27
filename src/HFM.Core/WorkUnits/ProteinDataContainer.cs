using HFM.Core.Data;
using HFM.Preferences;
using HFM.Proteins;

namespace HFM.Core.WorkUnits;

public class ProteinDataContainer : FileSerializerDataContainer<List<Protein>>
{
    private readonly IPreferences _preferences;

    public ProteinDataContainer(IPreferences preferences)
    {
        _preferences = preferences;
    }

    public override Serializers.IFileSerializer<List<Protein>> FileSerializer => new TabSerializer();

    public const string DefaultFileName = "ProjectInfo.tab";

    private string? _filePath;

    public override string FilePath =>
        _filePath ??= Path.Combine(_preferences.Get<string>(Preference.ApplicationDataFolderPath)!, DefaultFileName);

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private class TabSerializer : Serializers.IFileSerializer<List<Protein>>
    {
        private readonly TabDelimitedTextSerializer _serializer = new();

        public string FileExtension => "tab";

        public string FileTypeFilter => "Project Info Tab Delimited Files|*.tab";

        public List<Protein> Deserialize(string path)
        {
            var result = _serializer.ReadFile(path);
            return result as List<Protein> ?? result.ToList();
        }

        public void Serialize(string path, List<Protein>? value) =>
            _serializer.WriteFile(path, value);
    }
}
