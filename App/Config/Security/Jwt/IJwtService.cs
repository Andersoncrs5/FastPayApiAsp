using App.Modules.Role.Model;
using App.Modules.User.Model;

namespace App.Config.Security.Jwt;

public interface IJwtService
{
    AccessTokenResponse CreateToken(UserEntity user, List<RoleEntity> roles);
}