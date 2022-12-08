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

    //public static bool Validate(ClientRetrievalTask task) =>
    //    task.Interval is >= MinInterval and <= MaxInterval;

    //public const int MinInterval = 1;
    //public const int MaxInterval = 180;
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

    //public static bool Validate(WebGenerationTask task) =>
    //    task.Interval is >= MinInterval and <= MaxInterval;

    //public const int MinInterval = 1;
    //public const int MaxInterval = 180;
}
