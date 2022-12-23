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

public class FahClientGpuSlotDescription : FahClientSlotDescription
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

    private bool SetGpuBusAndSlot(string description)
    {
        var match = Regex.Match(description, @"gpu\:(?<GPUBus>\d+)\:(?<GPUSlot>\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
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

    private void SetDevice(string description)
    {
        var match = Regex.Match(description, @"gpu\:(?<GPUDevice>\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (match.Success &&
            Int32.TryParse(match.Groups["GPUDevice"].Value, out var device))
        {
            GpuDevice = device;
        }
    }

    private void SetGpuPrefix(string description)
    {
        var match = Regex.Match(description, @"gpu\:\d+\:\d+ (?<GPUPrefix>.+) \[", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (match.Success)
        {
            GpuPrefix = match.Groups["GPUPrefix"].Value;
        }
        else
        {
            match = Regex.Match(description, @"gpu\:\d+\:(?<GPUPrefix>.+) \[", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                GpuPrefix = match.Groups["GPUPrefix"].Value;
            }
        }
    }

    private void SetProcessor(string description)
    {
        var match = Regex.Match(description, "\\[(?<GPU>.+)\\]", RegexOptions.Singleline);
        if (match.Success)
        {
            Processor = match.Groups["GPU"].Value;
        }
    }
}

public class FahClientCpuSlotDescription : FahClientSlotDescription
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

    private void SetCpuThreads(string description)
    {
        var match = Regex.Match(description, @"[cpu|smp]\:(?<CPUThreads>\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (match.Success && Int32.TryParse(match.Groups["CPUThreads"].Value, out var threads))
        {
            CpuThreads = threads;
        }
    }
}
