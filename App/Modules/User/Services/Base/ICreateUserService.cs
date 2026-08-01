using App.Modules.User.Dto.Requests;
using App.Modules.User.Model;
using App.Utils.Result;

namespace App.Modules.User.Services.Base;

public interface ICreateUserService
{
    Task<Result<UserEntity>> Execute(CreateUserDto dto);
}