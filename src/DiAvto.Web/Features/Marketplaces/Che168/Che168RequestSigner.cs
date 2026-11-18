using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DiAvto.Web.Features.Marketplaces.Che168;

/// <summary>
/// Reproduces the signature used by the che168 mobile web client:
/// AES-192-ECB secret + sorted key/value concatenation + MD5.
/// The signature is computed over raw (non URL-encoded) values.
/// </summary>
public static class Che168RequestSigner
{
    private const string EncryptedSecret = "RKtjP64/s3lAOVCm1JzyOQ==";
    private const string KeyMaterial = "F941kK8J32k0jkL4D398ff53d0l3lhk7";

    private static readonly int[] KeyPermutation =
    [
        0, 6, 16, 4, 25, 5, 29, 24, 7, 31, 11, 30, 19, 14, 10, 17,
        20, 18, 12, 13, 8, 21, 28, 1, 26, 2, 27, 3, 9, 23, 15, 22
    ];

    private static readonly string Secret = DecryptSecret();

    public static IReadOnlyDictionary<string, string> BuildSignedParameters(
        IEnumerable<KeyValuePair<string, string?>> parameters,
        string appId,
        string appVersion,
        string subAppId)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in parameters)
        {
            if (!string.IsNullOrWhiteSpace(key) && value is not null)
            {
                values[key] = value;
            }
        }

        AddIfMissing(values, "_appid", appId);
        AddIfMissing(values, "v", appVersion);
        AddIfMissing(values, "_subappid", subAppId);

        var sorted = values
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToList();

        var payload = BuildSignaturePayload(sorted);
        var signature = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        values["_sign"] = signature;

        return values;
    }

    public static string BuildQueryString(IReadOnlyDictionary<string, string> signedParameters)
    {
        return string.Join("&", signedParameters.Select(x =>
            $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
    }

    public static string BuildUrl(
        string baseUrl,
        string path,
        IEnumerable<KeyValuePair<string, string?>> parameters,
        string appId,
        string appVersion,
        string subAppId)
    {
        var signed = BuildSignedParameters(parameters, appId, appVersion, subAppId);
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}?{BuildQueryString(signed)}";
    }

    public static string ComputeSignature(IReadOnlyDictionary<string, string> parameters)
    {
        var sorted = parameters
            .Where(x => x.Key != "_sign")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToList();

        var payload = BuildSignaturePayload(sorted);
        return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    private static string BuildSignaturePayload(IReadOnlyList<KeyValuePair<string, string>> sorted)
    {
        var builder = new StringBuilder(Secret.Length * 2 + sorted.Count * 24);
        builder.Append(Secret);

        foreach (var (key, value) in sorted)
        {
            builder.Append(key);
            builder.Append(value);
        }

        builder.Append(Secret);
        return builder.ToString();
    }

    private static void AddIfMissing(IDictionary<string, string> values, string key, string value)
    {
        if (!values.ContainsKey(key))
        {
            values[key] = value;
        }
    }

    private static string DecryptSecret()
    {
        var permuted = string.Concat(KeyPermutation.Select(index => KeyMaterial[index]));
        var key = Convert.FromBase64String(permuted);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var cipher = Convert.FromBase64String(EncryptedSecret);
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    public static string FormatWan(decimal fullCny)
    {
        return (fullCny / 10_000m).ToString("0.##", CultureInfo.InvariantCulture);
    }
}
