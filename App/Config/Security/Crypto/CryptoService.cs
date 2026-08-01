using System.Security.Cryptography;
using System.Text;
using App.Config.Options;
using Microsoft.Extensions.Options;

namespace App.Config.Security.Crypto;

public sealed class CryptoService(
    IOptions<CryptoOptions> options) : ICryptoService
{
    private readonly CryptoOptions _options = options.Value;

    public string GenerateToken()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            _options.TokenSize);

        Span<byte> bytes = stackalloc byte[_options.TokenSize];

        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public string ComputeSha256Hash(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(hash);
    }

    public bool VerifySha256Hash(string value, string hash)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(hash);

        string computedHash = ComputeSha256Hash(value);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(hash));
    }
}