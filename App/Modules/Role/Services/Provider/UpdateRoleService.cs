using App.Config.Database;
using App.Config.uow;
using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Mapper;
using App.Modules.Role.Model;
using App.Modules.Role.Services.Base;
using App.Utils.Result;
using Npgsql;

namespace App.Modules.Role.Services.Provider;

public class UpdateRoleService(
    IUnitOfWork uow,
    RoleMapper mapper
): IUpdateRoleService
{
    public async Task<Result<RoleEntity>> ExecuteAsync(long id, UpdateRoleDto dto)
    {
        RoleEntity? role = await uow.RoleRepository.GetByIdAsync(id);
        if (role == null)
            return Result<RoleEntity>.NotFound("Role not found");
        
        mapper.Update(dto, role);

        role.NormalizedName = dto.NormalizedName != null ? dto.NormalizedName.ToUpper() : role.NormalizedName;
        
        try
        {
            await uow.RoleRepository.UpdateAsync(role);

            return Result<RoleEntity>.Ok(200, role);
        }
        catch (PostgresException ex)
        {
            throw PostgresExceptionHandler.Handle(ex);
        }
        
    }
}