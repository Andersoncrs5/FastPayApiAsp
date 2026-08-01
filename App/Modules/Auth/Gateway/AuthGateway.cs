using App.Config.Security.Jwt;
using App.Modules.RefreshToken.Dto.Responses;
using App.Modules.RefreshToken.Services.Base;
using App.Modules.Role.Model;
using App.Modules.Role.Services.Base;
using App.Modules.User.Dto.Requests;
using App.Modules.User.Model;
using App.Modules.User.Services.Base;
using App.Modules.UserRole.Services.Base;
using App.Utils.Result;

namespace App.Modules.Auth.Gateway;

public class AuthGateway(
    ICreateUserService createUser,
    IJwtService jwtService,
    IFindRoleIdsByUserIdService findAllUseRoleByUserIdService,
    IFindRolesByIdsService findRolesByIds,
    ICreateRefreshTokenService createRefreshTokenService 
)
{
    public async Task<Result<RefreshTokenResult>> CreateRefreshToken(UserEntity user)
    {
        var result = await createRefreshTokenService.Execute(user.Id);
        
        return result;
    }
    
    public async Task<Result<UserEntity>> CreateUserAsync(CreateUserDto dto)
    {
        return await createUser.Execute(dto);
    }

    public AccessTokenResponse CreateToken(UserEntity user, List<RoleEntity> roles)
    {
        return jwtService.CreateToken(user, roles);
    }

    public async Task<Result<List<RoleEntity>>> GetRoles(UserEntity user)
    {
        return await findAllUseRoleByUserIdService.Execute(user.Id)
            .IfFailure(
                err => Result<List<RoleEntity>>.Failure(err.Errors, err.StatusCode),
                ids => findRolesByIds.Execute(ids));
    }
    
}