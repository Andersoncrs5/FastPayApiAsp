using App.Modules.User.Model;
using App.Utils.Base.Entity;

namespace App.Modules.RefreshToken.Model;

public class RefreshTokenEntity : BaseEntity
{
    public const string _tableName = "refresh_tokens";

    public required long UserId { get; set; }

    public required string TokenHash { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    
}