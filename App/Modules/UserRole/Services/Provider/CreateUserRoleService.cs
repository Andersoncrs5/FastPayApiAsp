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

public class CreateUserRoleService(
    IUnitOfWork uow,
    UserRoleGateway gateway,
    UserRoleMapper mapper
    ): ICreateUserRoleService
{
    public async Task<Result<UserRoleEntity>> Execute(CreateUserRoleDto dto)
    {
        UserRoleEntity entity = mapper.ToEntity(dto);
        
        try
        {
            await uow.UserRoleRepository.CreateAsync(entity);

            return Result<UserRoleEntity>.Created(entity);
        }
        catch (PostgresException ex)
        {
            throw PostgresExceptionHandler.Handle(ex);
        }
    }
}