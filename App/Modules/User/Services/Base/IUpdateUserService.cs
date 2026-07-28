using App.Modules.User.Dto.Requests;
using App.Modules.User.Model;
using App.Utils.Result;

namespace App.Modules.User.Services.Base;

public interface IUpdateUserService
{
    Task<Result<UserEntity>> Execute(long id, UpdateUserDto dto);
}