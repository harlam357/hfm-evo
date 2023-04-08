﻿using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace HFM.Core.Artifacts.SlotMarkup;

[DataContract(Namespace = "")]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class LogLine
{
    [DataMember(Order = 1)]
    public int Index { get; set; }

    [DataMember(Order = 2)]
    public string? Raw { get; set; }

    public override string? ToString() => Raw;

    // https://stackoverflow.com/a/36885492/425465
    public static string RemoveInvalidXmlChars(string value)
    {
        if (String.IsNullOrEmpty(value))
        {
            return value;
        }

        StringBuilder? result = null;
        for (int i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (XmlConvert.IsXmlChar(ch))
            {
                result?.Append(ch);
            }
            else if (result == null)
            {
                result = new();
                result.Append(value.AsSpan(0, i));
            }
        }

        return result == null ? value : result.ToString();
    }
}
