using App.Modules.User.Dto.Requests;
using App.Modules.User.Dto.Responses;
using App.Modules.User.Model;
using Riok.Mapperly.Abstractions;

namespace App.Modules.User.Mapper;

[Mapper]
public partial class UserMapper
{
    public partial UserEntity ToEntity(CreateUserDto dto);

    public partial void Update(UpdateUserDto dto, UserEntity entity);

    public partial UserResponse ToResponse(UserEntity entity);
}