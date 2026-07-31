using App.Config.Database;
using App.Config.Tx;
using App.Modules.Role.Model;
using App.Modules.Role.Repositories;
using App.Modules.User.Model;
using App.Modules.User.Repositories;
using App.Modules.UserRole.Model;
using App.Modules.UserRole.Repositories;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests.Config;

[CollectionDefinition("Integration Tests")]
public class IntegrationCollection : ICollectionFixture<DatabaseFixture>
{
}

[Collection("Integration Tests")]
public abstract class BaseIntegrationTest : IAsyncDisposable
{
    protected readonly HttpClient Client;
    protected readonly DatabaseFixture Factory;
    protected readonly Faker Faker = new("pt_BR");

    protected readonly IServiceScope Scope;
    protected readonly UserRepository UserRepository;
    protected readonly RoleRepository RoleRepository;
    protected readonly UserRoleRepository UserRoleRepository;

    protected BaseIntegrationTest(DatabaseFixture factory)
    {
        Factory = factory;

        Client = factory.CreateClient();

        Scope = factory.Services.CreateScope();

        IDatabase database =
            Scope.ServiceProvider.GetRequiredService<IDatabase>();
        
        var session = new TestDbSession(database);
        
        UserRepository = new UserRepository(session);
        RoleRepository = new RoleRepository(session);
        UserRoleRepository = new UserRoleRepository(session);
    }

    protected async Task<UserRoleEntity> CreateUserRole(long userId, long roleId, long? assignedById = null)
    {
        var entity = new UserRoleEntity
        {
            Id = Faker.Random.Long(10_000_000, 9_999_999_999),
            UserId = userId,
            RoleId = roleId,
            Active = true,
            AssignedByUserId = assignedById,
            RevokedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await UserRoleRepository.CreateAsync(entity);

        return entity;
    }
    
    public async Task<RoleEntity> CreateRole(bool active = true)
    {
        long id = Faker.Random.Long(100000000, 99999999999);
        string name = Faker.Name.FindName();
        var date = DateTimeOffset.UtcNow;
        
        var role = new RoleEntity
        {
            Id = id,
            Name = name,
            Description = Faker.Lorem.Sentence(),
            NormalizedName = name.ToUpper(),
            Active = active,
            CreatedAt = date,
            UpdatedAt = date
        };

        await RoleRepository.CreateAsync(role);
        
        return role;
    }
    
    public async Task<UserEntity> CreateUser()
    {
        var id = Faker.Random.Long(10000000, 9999999999);

        var user = new UserEntity
        {
            Id = id,
            UserName = "Pochita " + id,
            Email = Faker.Internet.Email(),
            FullName = Faker.Name.FullName(),
            PasswordHash = Faker.Internet.Password(),
            NormalizedUserName = Faker.Internet.UserName(),
            NormalizedEmail = Faker.Internet.Email(),
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await UserRepository.CreateAsync(user);

        return user;
    }


    public async ValueTask DisposeAsync()
    {
        if (Scope is IAsyncDisposable asyncScope)
        {
            await asyncScope.DisposeAsync();
        }
        else
        {
            Scope.Dispose();
        }
    }
}