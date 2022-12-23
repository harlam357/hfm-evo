using System.Text.RegularExpressions;

namespace HFM.Core.Client;

public abstract class FahClientSlotDescription
{
    public abstract FahClientSlotType SlotType { get; }

    public string? Processor { get; set; }

    public string? Value { get; set; }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static FahClientSlotDescription? Parse(string? value) =>
        GetSlotType(value) switch
        {
            FahClientSlotType.Gpu => FahClientGpuSlotDescription.Parse(value!),
            FahClientSlotType.Cpu => FahClientCpuSlotDescription.Parse(value!),
            FahClientSlotType.Unknown => null,
            _ => null
        };

    private static FahClientSlotType GetSlotType(string? value) =>
        String.IsNullOrWhiteSpace(value)
            ? FahClientSlotType.Unknown
            : value.StartsWith("gpu", StringComparison.OrdinalIgnoreCase)
                ? FahClientSlotType.Gpu
                : FahClientSlotType.Cpu;
}

public partial class FahClientGpuSlotDescription : FahClientSlotDescription
{
    public override FahClientSlotType SlotType => FahClientSlotType.Gpu;

    public int? GpuBus { get; set; }
    public int? GpuSlot { get; set; }
    public int? GpuDevice { get; set; }
    public string? GpuPrefix { get; set; }

    public static new FahClientGpuSlotDescription Parse(string value)
    {
        var d = new FahClientGpuSlotDescription
        {
            Value = value
        };

        if (!d.SetGpuBusAndSlot(value))
        {
            d.SetDevice(value);
        }
        d.SetGpuPrefix(value);
        d.SetProcessor(value);

        return d;
    }

    [GeneratedRegex("gpu\\:(?<GPUBus>\\d+)\\:(?<GPUSlot>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex GpuBusAndSlotRegex();

    private bool SetGpuBusAndSlot(string description)
    {
        var match = GpuBusAndSlotRegex().Match(description);
        if (match.Success &&
            Int32.TryParse(match.Groups["GPUBus"].Value, out var bus) &&
            Int32.TryParse(match.Groups["GPUSlot"].Value, out var slot))
        {
            GpuBus = bus;
            GpuSlot = slot;
            return true;
        }

        return false;
    }

    [GeneratedRegex("gpu\\:(?<GPUDevice>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex DeviceRegex();

    private void SetDevice(string description)
    {
        var match = DeviceRegex().Match(description);
        if (match.Success &&
            Int32.TryParse(match.Groups["GPUDevice"].Value, out var device))
        {
            GpuDevice = device;
        }
    }

    [GeneratedRegex("gpu\\:\\d+\\:\\d+ (?<GPUPrefix>.+) \\[", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex GpuBusAndSlotPrefixRegex();

    [GeneratedRegex("gpu\\:\\d+\\:(?<GPUPrefix>.+) \\[", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex GpuDevicePrefixRegex();

    private void SetGpuPrefix(string description)
    {
        var match = GpuBusAndSlotPrefixRegex().Match(description);
        if (match.Success)
        {
            GpuPrefix = match.Groups["GPUPrefix"].Value;
        }
        else
        {
            match = GpuDevicePrefixRegex().Match(description);
            if (match.Success)
            {
                GpuPrefix = match.Groups["GPUPrefix"].Value;
            }
        }
    }

    [GeneratedRegex("\\[(?<GPU>.+)\\]", RegexOptions.Singleline)]
    private static partial Regex ProcessorRegex();

    private void SetProcessor(string description)
    {
        var match = ProcessorRegex().Match(description);
        if (match.Success)
        {
            Processor = match.Groups["GPU"].Value;
        }
    }
}

public partial class FahClientCpuSlotDescription : FahClientSlotDescription
{
    public override FahClientSlotType SlotType => FahClientSlotType.Cpu;

    public int? CpuThreads { get; set; }

    public static new FahClientCpuSlotDescription Parse(string value)
    {
        var d = new FahClientCpuSlotDescription
        {
            Value = value
        };

        d.SetCpuThreads(value);

        return d;
    }

    [GeneratedRegex("[cpu|smp]\\:(?<CPUThreads>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex CpuThreadsRegex();

    private void SetCpuThreads(string description)
    {
        var match = CpuThreadsRegex().Match(description);
        if (match.Success && Int32.TryParse(match.Groups["CPUThreads"].Value, out var threads))
        {
            CpuThreads = threads;
        }
    }
}
