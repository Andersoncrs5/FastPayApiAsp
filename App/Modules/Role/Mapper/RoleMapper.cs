using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Dto.Responses;
using App.Modules.Role.Model;
using Riok.Mapperly.Abstractions;

namespace App.Modules.Role.Mapper;

[Mapper]
public partial class RoleMapper
{
    public partial RoleEntity ToEntity(CreateRoleDto dto);

    public partial void Update(UpdateRoleDto dto, RoleEntity entity);

    public partial RoleDto ToResponse(RoleEntity entity);
}