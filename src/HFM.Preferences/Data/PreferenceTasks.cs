using System.Runtime.Serialization;

namespace HFM.Preferences.Data;

[DataContract(Namespace = "")]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record ClientRetrievalTask()
{
    public ClientRetrievalTask(ClientRetrievalTask other)
    {
        Enabled = other.Enabled;
        Interval = other.Interval;
        ProcessingMode = other.ProcessingMode;
    }

    [DataMember]
    public bool Enabled { get; set; } = true;

    [DataMember]
    public int Interval { get; set; } = 15;

    [DataMember]
    public string? ProcessingMode { get; set; }
}

[DataContract(Namespace = "")]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record WebGenerationTask()
{
    public WebGenerationTask(WebGenerationTask other)
    {
        Enabled = other.Enabled;
        Interval = other.Interval;
        AfterClientRetrieval = other.AfterClientRetrieval;
    }

    [DataMember]
    public bool Enabled { get; set; }

    [DataMember]
    public int Interval { get; set; } = 15;

    [DataMember]
    public bool AfterClientRetrieval { get; set; }
}
