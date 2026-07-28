using App.Config.Security;
using App.Config.uow;
using App.Modules.User.Dto.Requests;
using App.Modules.User.Mapper;
using App.Modules.User.Model;
using App.Modules.User.Services.Base;
using App.Utils.Result;
using Npgsql;

namespace App.Modules.User.Services.Provider;

public class CreateUserService(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    UserMapper mapper
    ): ICreateUserService
{
    public async Task<Result<UserEntity>> Execute(CreateUserDto dto)
    {
        UserEntity user = mapper.ToEntity(dto);
     
        user.PasswordHash = passwordHasher.Hash(dto.Password);
        
        try
        {
            await uow.UserRepository.CreateAsync(user);

            return Result<UserEntity>.Success(user, 201);
        }
        catch (PostgresException ex) when (ex.ConstraintName == "ux_users_username")
        {
            return Result<UserEntity>.Failure(
                "Username already exists",
                409);
        }
        catch (PostgresException ex) when (ex.ConstraintName == "ux_users_email")
        {
            return Result<UserEntity>.Failure(
                "Email already exists",
                409);
        }
        
    }
}