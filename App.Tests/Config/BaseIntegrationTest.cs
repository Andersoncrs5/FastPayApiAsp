using App.Config.Database;
using App.Modules.User.Model;
using App.Modules.User.Repositories;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests.Config;

[CollectionDefinition("Integration Tests")]
public class IntegrationCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Integration Tests")]
public abstract class BaseIntegrationTest
{
    protected readonly HttpClient Client;
    protected readonly DatabaseFixture Factory;
    protected readonly Faker _faker = new("pt_BR");
    protected readonly UserRepository _userRepository;
    
    protected BaseIntegrationTest(DatabaseFixture factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        using var scope = factory.Services.CreateScope();
        IDatabase database = scope.ServiceProvider.GetRequiredService<IDatabase>();

        _userRepository = new UserRepository(database);
    }

    public async Task<UserEntity> CreateUser() {
        var id = _faker.Random.Long(10000000, 9999999999);
        var user = new UserEntity
        {
            Id = id, 
            UserName = "Pochita " + id,
            Email = _faker.Internet.Email(),
            FullName = _faker.Name.FullName(),
            PasswordHash = _faker.Internet.Password(),
            NormalizedUserName = _faker.Internet.UserName(),
            NormalizedEmail = _faker.Internet.Email(),
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.CreateAsync(user);
        
        return user;
    }
    
}