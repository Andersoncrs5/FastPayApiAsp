using App.Config.Database;
using App.Config.Security;
using App.Config.uow;
using App.Modules.User.Dto.Requests;
using App.Modules.User.Mapper;
using App.Modules.User.Model;
using App.Modules.User.Services.Base;
using App.Utils.Result;
using Npgsql;

namespace App.Modules.User.Services.Provider;

public class UpdateUserService(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    UserMapper mapper
): IUpdateUserService
{
    public async Task<Result<UserEntity>> Execute(long id, UpdateUserDto dto)
    {
        UserEntity? user = await uow.UserRepository.GetByIdAsync(id);
        if (user == null) return Result<UserEntity>.Failure("User not found", 404);
        
        try
        {
            mapper.Update(dto, user);
            
            if (dto.Password != null) 
                user.PasswordHash = passwordHasher.Hash(dto.Password);
            
            await uow.UserRepository.UpdateAsync(user);
            
            return Result<UserEntity>.Ok(200, user);
        }
        catch (PostgresException ex)
        {
            throw PostgresExceptionHandler.Handle(ex);
        }
    }
}