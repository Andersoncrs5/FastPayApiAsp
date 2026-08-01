namespace App.Config.Options;

public sealed class CryptoOptions
{
    public const string SectionName = "Crypto";

    public int TokenSize { get; init; } = 64;
}