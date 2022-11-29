using System.Globalization;

namespace HFM.Preferences;

public static class FormColumnPreference
{
    public static (int DisplayIndex, int Width, bool Visible, int Index)? Parse(string value)
    {
        string[] tokens = value.Split(',');
        if (tokens.Length != 4)
        {
            return null;
        }

        try
        {
            var cultureInfo = CultureInfo.InvariantCulture;
            return (
                Int32.Parse(tokens[0], cultureInfo),
                Int32.Parse(tokens[1], cultureInfo),
                Boolean.Parse(tokens[2]),
                Int32.Parse(tokens[3], cultureInfo));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static string Format(int displayIndex, int width, bool visible, int index) =>
        String.Format(CultureInfo.InvariantCulture,
            "{0:D2},{1},{2},{3}",
            displayIndex,
            width,
            visible,
            index);
}
