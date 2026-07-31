using App.Modules.Role.Repositories;
using App.Modules.User.Repositories;
using App.Modules.UserRole.Repositories;

namespace App.Config.uow;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    IUserRoleRepository UserRoleRepository { get; }
    
    Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default);
}