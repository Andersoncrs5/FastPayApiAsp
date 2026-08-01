using App.Config.Database;
using App.Config.Options;
using App.Config.Tx;
using App.Modules.RefreshToken.Repositories;
using App.Modules.Role.Repositories;
using App.Modules.User.Repositories;
using App.Modules.UserRole.Repositories;
using App.Utils.Result;
using IdGen;
using Microsoft.Extensions.Options;
using Npgsql;

namespace App.Config.uow;

public sealed class UnitOfWork(
    IRequestDbContext database,
    IdGenerator idGenerator,
    IOptions<DatabaseOptions> options
    ) : IUnitOfWork
{
    private IUserRepository? _userRepository;
    private IRoleRepository? _roleRepository;
    private IUserRoleRepository? _userRoleRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;
    
    public IUserRepository UserRepository
        => _userRepository ??= new UserRepository(database);
    public IRoleRepository RoleRepository
        => _roleRepository ??= new RoleRepository(database);
    public IUserRoleRepository UserRoleRepository
        => _userRoleRepository ??= new UserRoleRepository(database);
    public IRefreshTokenRepository RefreshTokenRepository
        => _refreshTokenRepository ??= new RefreshTokenRepository(database);

    public IdGenerator IdGenerator { get; } = idGenerator;
    
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