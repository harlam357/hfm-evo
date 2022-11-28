﻿using System.Security.Cryptography;
using System.Text;

namespace HFM.Core.Internal;

internal class Cryptography
{
    private string SymmetricKey { get; }
    private string InitializationVector { get; }

    internal Cryptography(string symmetricKey, string initializationVector)
    {
        SymmetricKey = symmetricKey;
        InitializationVector = initializationVector;
    }

    internal string DecryptValue(string? value)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            return String.Empty;
        }

        string result;

        try
        {
            result = DecryptInternal(value);
        }
        catch (FormatException)
        {
            // return the value as is
            result = value;
        }
        catch (CryptographicException)
        {
            // return the value as is
            result = value;
        }

        return result;
    }

    private string DecryptInternal(string value)
    {
        string plainText;
        using (var algorithm = CreateSymmetricAlgorithm())
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(value)))
            {
                using (var cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        plainText = sr.ReadToEnd();
                    }
                }
            }
        }

        return plainText;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal string EncryptValue(string? value)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            return String.Empty;
        }

        string result;

        try
        {
            result = EncryptInternal(value);
        }
        catch (CryptographicException)
        {
            // return the value as is
            result = value;
        }

        return result;
    }

    private string EncryptInternal(string value)
    {
        byte[] encrypted;
        using (var algorithm = CreateSymmetricAlgorithm())
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(value);
                    }
                    encrypted = ms.ToArray();
                }
            }
        }

        return Convert.ToBase64String(encrypted);
    }

    private SymmetricAlgorithm CreateSymmetricAlgorithm()
    {
#pragma warning disable SYSLIB0022
        var algorithm = new RijndaelManaged();
#pragma warning restore SYSLIB0022
        SetKey(algorithm, Encoding.UTF8.GetBytes(SymmetricKey));
        SetIV(algorithm, Encoding.UTF8.GetBytes(InitializationVector));
        return algorithm;
    }

    private static void SetKey(SymmetricAlgorithm algorithm, byte[] key)
    {
        int minBytes = algorithm.LegalKeySizes[0].MinSize / 8;
        int maxBytes = algorithm.LegalKeySizes[0].MaxSize / 8;
        algorithm.Key = ResizeByteArray(key, minBytes, maxBytes);
    }

    private static void SetIV(SymmetricAlgorithm algorithm, byte[] iv)
    {
        int minBytes = algorithm.BlockSize / 8;
        int maxBytes = algorithm.BlockSize / 8;
        algorithm.IV = ResizeByteArray(iv, minBytes, maxBytes);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static byte[] ResizeByteArray(byte[] bytes, int minBytes, int maxBytes)
    {
        if (maxBytes > 0)
        {
            if (bytes.Length > maxBytes)
            {
                var b = new byte[maxBytes];
                Array.Copy(bytes, b, b.Length);
                bytes = b;
            }
        }
        if (minBytes > 0)
        {
            if (bytes.Length < minBytes)
            {
                var b = new byte[minBytes];
                Array.Copy(bytes, b, bytes.Length);
                bytes = b;
            }
        }
        return bytes;
    }
}
