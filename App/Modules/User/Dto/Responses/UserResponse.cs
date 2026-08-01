using App.Utils.Base.Dto;

namespace App.Modules.User.Dto.Responses;

public class UserResponse: BaseDto
{
   public string UserName { get; set; }
   public string Email { get; set; }
}