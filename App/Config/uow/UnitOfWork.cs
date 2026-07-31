using App.Config.Database;
using App.Config.Options;
using App.Config.Tx;
using App.Modules.Role.Repositories;
using App.Modules.User.Repositories;
using App.Modules.UserRole.Repositories;
using App.Utils.Result;
using Microsoft.Extensions.Options;
using Npgsql;

namespace App.Config.uow;

public sealed class UnitOfWork(
    IRequestDbContext database,
    IOptions<DatabaseOptions> options
    ) : IUnitOfWork
{
    private IUserRepository? _userRepository;
    private IRoleRepository? _roleRepository;
    private IUserRoleRepository? _userRoleRepository;
    
    public IUserRepository UserRepository
        => _userRepository ??= new UserRepository(database);
    public IRoleRepository RoleRepository
        => _roleRepository ??= new RoleRepository(database);
    public IUserRoleRepository UserRoleRepository
        => _userRoleRepository ??= new UserRoleRepository(database);
    
    private readonly string _connectionString = options.Value.Postgres; 
    
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action(cancellationToken);

            if (result is IResultState state && state.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}