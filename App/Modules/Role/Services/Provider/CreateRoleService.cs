using App.Config.Database;
using App.Config.uow;
using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Mapper;
using App.Modules.Role.Model;
using App.Modules.Role.Services.Base;
using App.Utils.Result;
using Npgsql;

namespace App.Modules.Role.Services.Provider;

public class CreateRoleService(
    IUnitOfWork uow,
    RoleMapper mapper
): ICreateRoleService
{
    public async Task<Result<RoleEntity>> Execute(CreateRoleDto dto)
    {
        RoleEntity role = mapper.ToEntity(dto);
        role.NormalizedName = dto.NormalizedName == null ? role.Name.ToUpper() : dto.NormalizedName.ToUpper();

        try
        {
            await uow.RoleRepository.CreateAsync(role);

            return Result<RoleEntity>.Created(role);
        }
        catch (PostgresException ex)
        {
            throw PostgresExceptionHandler.Handle(ex);
        }
    }
}