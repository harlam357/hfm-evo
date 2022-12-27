using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HFM.Preferences;
using HFM.Proteins;

namespace HFM.Core.WorkUnits;

[TestFixture]
public class ProteinDataContainerTests
{
    [Test]
    public void ReadsFromFile()
    {
        var preferences = new InMemoryPreferencesProvider("", TestFiles.ProjectPath, "");
        var dataContainer = new ProteinDataContainer(preferences);
        dataContainer.Read();
        Assert.That(dataContainer.Data, Has.Count.Not.EqualTo(0));
    }

    [Test]
    public void WritesToFile()
    {
        using var artifacts = new ArtifactFolder();
        var preferences = new InMemoryPreferencesProvider("", artifacts.Path, "");
        var dataContainer = new ProteinDataContainer(preferences)
        {
            Data = new List<Protein>
            {
                new() { ProjectNumber = 1 }
            }
        };
        dataContainer.Write();

        Assert.That(File.Exists(Path.Combine(artifacts.Path, "ProjectInfo.tab")));
    }
}
