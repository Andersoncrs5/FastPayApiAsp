using App.Config.Database;
using App.Config.Tx;
using App.Modules.User.Model;
using App.Modules.User.Repositories;
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

    protected BaseIntegrationTest(DatabaseFixture factory)
    {
        Factory = factory;

        Client = factory.CreateClient();

        Scope = factory.Services.CreateScope();

        IDatabase database =
            Scope.ServiceProvider.GetRequiredService<IDatabase>();
        
        var session = new TestDbSession(database);

        
        UserRepository = new UserRepository(session);
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