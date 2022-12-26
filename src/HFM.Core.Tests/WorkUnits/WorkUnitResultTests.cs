namespace HFM.Core.WorkUnits;

[TestFixture]
public class WorkUnitResultTests
{
    [TestCase(0, ExpectedResult = false)]
    [TestCase(1, ExpectedResult = true)]
    [TestCase(2, ExpectedResult = true)]
    [TestCase(3, ExpectedResult = true)]
    [TestCase(4, ExpectedResult = false)]
    [TestCase(5, ExpectedResult = true)]
    [TestCase(6, ExpectedResult = false)]
    [TestCase(7, ExpectedResult = true)]
    [TestCase(8, ExpectedResult = false)]
    [TestCase(9, ExpectedResult = false)]
    [TestCase(10, ExpectedResult = true)]
    public bool IsTerminating(int value)
    {
        var result = new WorkUnitResult(value);
        return result.IsTerminating;
    }

    [TestCase(0, ExpectedResult = "UNKNOWN")]
    [TestCase(1, ExpectedResult = "FINISHED_UNIT")]
    [TestCase(2, ExpectedResult = "EARLY_UNIT_END")]
    [TestCase(3, ExpectedResult = "UNSTABLE_MACHINE")]
    [TestCase(4, ExpectedResult = "INTERRUPTED")]
    [TestCase(5, ExpectedResult = "BAD_WORK_UNIT")]
    [TestCase(6, ExpectedResult = "CORE_OUTDATED")]
    [TestCase(7, ExpectedResult = "UNKNOWN")]
    [TestCase(8, ExpectedResult = "GPU_MEMTEST_ERROR")]
    [TestCase(9, ExpectedResult = "UNKNOWN_ENUM")]
    [TestCase(10, ExpectedResult = "BAD_FRAME_CHECKSUM")]
    public string ToString(int value)
    {
        var result = new WorkUnitResult(value);
        return result.ToString();
    }

    [TestCase("UNKNOWN", ExpectedResult = 0)]
    [TestCase("FINISHED_UNIT", ExpectedResult = 1)]
    [TestCase("EARLY_UNIT_END", ExpectedResult = 2)]
    [TestCase("UNSTABLE_MACHINE", ExpectedResult = 3)]
    [TestCase("INTERRUPTED", ExpectedResult = 4)]
    [TestCase("BAD_WORK_UNIT", ExpectedResult = 5)]
    [TestCase("CORE_OUTDATED", ExpectedResult = 6)]
    [TestCase("GPU_MEMTEST_ERROR", ExpectedResult = 8)]
    [TestCase("UNKNOWN_ENUM", ExpectedResult = 9)]
    [TestCase("BAD_FRAME_CHECKSUM", ExpectedResult = 10)]
    public int Parse(string value)
    {
        var result = WorkUnitResult.Parse(value);
        return result.Value;
    }
}
