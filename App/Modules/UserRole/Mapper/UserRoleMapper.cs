using App.Modules.UserRole.Dto.Requests;
using App.Modules.UserRole.Dto.Responses;
using App.Modules.UserRole.Model;
using Riok.Mapperly.Abstractions;

namespace App.Modules.UserRole.Mapper;

[Mapper]
public partial class UserRoleMapper
{
    public partial void Update(UpdateUserRoleDto dto, UserRoleEntity entity);
    public partial UserRoleEntity ToEntity(CreateUserRoleDto dto);
    
    public partial UserRoleResponseDto ToResponse(UserRoleEntity entity);
}