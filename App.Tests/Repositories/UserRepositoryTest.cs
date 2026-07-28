using App.Tests.Config;
using FluentAssertions;
using Xunit.Abstractions;

namespace App.Tests.Repositories;

public class UserRepositoryTest: BaseIntegrationTest
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public UserRepositoryTest(DatabaseFixture fixture, ITestOutputHelper output) : base(fixture)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_ShouldCreateUser()
    {
        await CreateUser();
    }
    
    [Fact]
    public async Task ExistsById_Should_Return_False()
    {
        long id = Faker.Random.Long(100000, 999999);
        
        bool exists = await UserRepository.ExistsByIdAsync(id);
        
        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task ExistsById_Should_Return_True()
    {
        var user = await CreateUser();
        
        bool exists = await UserRepository.ExistsByIdAsync(user.Id);
        
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldDeleteUser()
    {
        var user = await CreateUser();
        
        await UserRepository.DeleteAsync(user.Id);
        
        bool exists = await UserRepository.ExistsByIdAsync(user.Id);
        
        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteAndCount_ShouldDeleteUser()
    {
        var user = await CreateUser();

        long count = await UserRepository.DeleteAndCountAsync(user.Id);

        bool exists = await UserRepository.ExistsByIdAsync(user.Id);
        
        exists.Should().BeFalse();
        count.Should().Be(1);
    }
    
    [Fact]
    public async Task Repository_Should_ExecuteAllBaseCrudOperationsSuccessfully()
    {
        var user = await CreateUser();

        var dbUser = await UserRepository.GetByIdAsync(user.Id);
        var exists = await UserRepository.ExistsByIdAsync(user.Id);

        dbUser.Should().NotBeNull();
        dbUser!.Id.Should().Be(user.Id);
        dbUser.Email.Should().Be(user.Email);
        dbUser.FullName.Should().Be(user.FullName);
        exists.Should().BeTrue();

        dbUser.FullName = "Nome Alterado via Teste";
        dbUser.UpdatedAt = DateTime.UtcNow;
        
        await UserRepository.UpdateAsync(dbUser);
        
        var updatedDbUser = await UserRepository.GetByIdAsync(user.Id);
        updatedDbUser!.FullName.Should().Be("Nome Alterado via Teste");

        var affectedRows = await UserRepository.DeleteAndCountAsync(user.Id);
        var existsAfterDelete = await UserRepository.ExistsByIdAsync(user.Id);

        affectedRows.Should().Be(1);
        existsAfterDelete.Should().BeFalse();
    }
    
    [Fact]
    public async Task ExistsByUsername_Should_Return_True_WhenUsernameHasSameCase()
    {
        var user = await CreateUser();

        bool exists = await UserRepository.ExistsByUsernameAsync(
            user.UserName);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUsername_Should_Return_True_WhenUsernameHasDifferentCase()
    {
        var user = await CreateUser();

        bool exists = await UserRepository.ExistsByUsernameAsync(
            user.UserName.ToUpperInvariant());

        exists.Should().BeTrue();
    }


    [Fact]
    public async Task ExistsByUsername_Should_Return_True_WhenUsernameIsLowerCase()
    {
        var user = await CreateUser();

        bool exists = await UserRepository.ExistsByUsernameAsync(
            user.UserName.ToLowerInvariant());

        exists.Should().BeTrue();
    }


    [Fact]
    public async Task ExistsByUsername_Should_Return_False_WhenUsernameDoesNotExist()
    {
        bool exists = await UserRepository.ExistsByUsernameAsync(
            "username_that_does_not_exist");

        exists.Should().BeFalse();
    }
    
}