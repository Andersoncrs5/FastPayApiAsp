using App.Config.Database;
using App.Config.uow;
using App.Modules.UserRole.Dto.Requests;
using App.Modules.UserRole.Gateway;
using App.Modules.UserRole.Mapper;
using App.Modules.UserRole.Model;
using App.Modules.UserRole.Services.Base;
using App.Utils.Result;
using Npgsql;

namespace App.Modules.UserRole.Services.Provider;

public class UpdateUserRoleService(
    IUnitOfWork uow,
    UserRoleGateway gateway,
    UserRoleMapper mapper
    ): IUpdateUserRoleService
{
    public async Task<Result<UserRoleEntity>> Execute(long userRoleId, UpdateUserRoleDto dto)
    {
        UserRoleEntity? entity = await uow.UserRoleRepository.GetByIdAsync(userRoleId);
        if (entity == null) return Result<UserRoleEntity>.NotFound("User not found");

        mapper.Update(dto, entity);
        
        try
        {
            await uow.UserRoleRepository.UpdateAsync(entity);

            return Result<UserRoleEntity>.Success(entity);
        }
        catch (PostgresException ex)
        {
            return PostgresResultHandler.Handle<UserRoleEntity>(ex);
        }
    }
    
}