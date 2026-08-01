using App.Config.Security.Crypto;

namespace App.Modules.RefreshToken.Gateway;

public sealed class RefreshTokenGateway(ICryptoService crypto)
{
    public string Hash(string token)
    {
        return crypto.ComputeSha256Hash(token);
    }
}