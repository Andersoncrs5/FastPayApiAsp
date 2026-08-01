namespace App.Config.Security.Crypto;

public interface ICryptoService
{
    string GenerateToken();

    string ComputeSha256Hash(string value);

    bool VerifySha256Hash(string value, string hash);
}