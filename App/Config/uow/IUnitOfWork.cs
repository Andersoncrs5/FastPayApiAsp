using App.Modules.RefreshToken.Repositories;
using App.Modules.Role.Repositories;
using App.Modules.User.Repositories;
using App.Modules.UserRole.Repositories;
using IdGen;

namespace App.Config.uow;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    IUserRoleRepository UserRoleRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IdGenerator IdGenerator { get; }
    
    Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default);
}